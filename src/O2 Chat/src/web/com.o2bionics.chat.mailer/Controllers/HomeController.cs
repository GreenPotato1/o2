using System;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;
using Com.O2Bionics.MailerService.Contract;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.Utils.Web;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json;

namespace Com.O2Bionics.MailerService.Web.Controllers
{
    public sealed class HomeController : Controller
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(HomeController));

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task Send(MailRequest request)
        {
            var subjectBody = Generate(request);
            if (null == subjectBody)
                return;

            var emailSender = GlobalContainer.Resolve<IEmailSender>();
            Debug.Assert(null != emailSender);
            try
            {
                await emailSender.Send(request.To, subjectBody.Subject, subjectBody.Body);

                if (m_log.IsDebugEnabled)
                    m_log.Debug($"Sent request: {request}");
            }
            catch (SmtpFailedRecipientException e)
            {
                m_log.WarnFormat("Failed recipient '{0}' to send to '{1}': {2}", e.FailedRecipient, request.To, e);
                Response.Write(HttpStatusCode.BadRequest, $"Can't send to provided email <{e.FailedRecipient}>: {e.Message}");
            }
            catch (SmtpException e)
            {
                m_log.WarnFormat("Failed send to '{0}': {1}", request.To, e);
                Response.Write(HttpStatusCode.BadRequest, $"Can't send to provided email: {e.Message}");
            }
        }

        // TODO: task-356. load templates by visitor's (or specified) culture using reflection.

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GenerateSubjectAndBody(MailRequest request)
        {
            var subjectBody = Generate(request);
            var result = null == subjectBody ? null : JilJson(subjectBody);
            return result;
        }

        [CanBeNull]
        private SubjectBody Generate([CanBeNull] MailRequest request)
        {
            if (!Validate(request)
                || !DeserializeModel(request, out var model)
                || !RenderRazorViewToString(request, model, out var subject, out var body))
                return null;

            Debug.Assert(!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(body));
            var result = new SubjectBody { Subject = subject, Body = body };
            return result;
        }

        private bool Validate(MailRequest request, bool shallRequireEmail = false)
        {
            if (null == request)
            {
                Response.Write(HttpStatusCode.BadRequest, "Input must be provided.");
                return false;
            }

            if (m_log.IsDebugEnabled)
                m_log.Debug($"Request: {request}");

            if (!CheckIdentifier(nameof(request.ProductCode), request.ProductCode) ||
                !CheckIdentifier(nameof(request.TemplateId), request.TemplateId))
                return false;

            if (shallRequireEmail && string.IsNullOrEmpty(request.To))
            {
                Response.Write(HttpStatusCode.BadRequest, $"{nameof(request.To)} must be provided.");
                return false;
            }

            return true;
        }

        private bool CheckIdentifier([NotNull] string name, [CanBeNull] string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Response.Write(HttpStatusCode.BadRequest, $"{name} must be provided.");
                return false;
            }

            var error = IdentifierHelper.LowerOrUpperCase(value);
            if (string.IsNullOrEmpty(error))
                return true;

            Response.Write(HttpStatusCode.BadRequest, $"{name} {error}");
            return false;
        }

        private bool DeserializeModel(MailRequest request, out ExpandoObject model)
        {
            model = null;
            if (string.IsNullOrEmpty(request.TemplateModel))
                return true;
            try
            {
                model = JsonConvert.DeserializeObject<ExpandoObject>(request.TemplateModel);
                return true;
            }
            catch (Exception e)
            {
                const string message = "Cannot deserialize the model.";
                try
                {
                    var error = $"{message} {request.TemplateModel.LimitLength(1 << 15)}";
                    m_log.Error(error, e);
                }
                catch
                {
                    //Ignore
                }

                Response.Write(HttpStatusCode.BadRequest, message);
            }

            return false;
        }

        private static string GetFilename(MailRequest request)
        {
            return $@"~\Views\{request.ProductCode}\{request.TemplateId}.cshtml";
        }

        private bool RenderRazorViewToString(MailRequest request, object model, out string subject, out string body)
        {
            var filename = GetFilename(request);
            ViewData.Model = model;
            subject = body = null;
            using (var writer = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, filename);
                var view = viewResult?.View;
                if (null == view)
                {
                    var error =
                        $"View must exist, {nameof(request.ProductCode)}='{request.ProductCode}', {nameof(request.TemplateId)}='{request.TemplateId}'.";
                    Response.Write(HttpStatusCode.BadRequest, error);
                    return false;
                }

                var viewContext = new ViewContext(ControllerContext, view, ViewData, TempData, writer);
                view.Render(viewContext, writer);
                viewResult.ViewEngine.ReleaseView(ControllerContext, view);
                body = writer.GetStringBuilder().ToString();
                if (string.IsNullOrEmpty(body))
                {
                    const string error = "The body must have been set by the template.";
                    Response.Write(HttpStatusCode.BadRequest, error);
                    return false;
                }

                var result = ViewData.TryGetValue(MailerConstants.SubjectKey, out var oSubject) && oSubject is string;
                subject = oSubject as string;
                if (result && !string.IsNullOrEmpty(subject))
                    return true;

                {
                    const string error = "The " + MailerConstants.SubjectKey + " must have been set by the template.";
                    Response.Write(HttpStatusCode.BadRequest, error);
                    return false;
                }
            }
        }

        private static JsonResult JilJson(object result)
        {
            return new JilJsonResult
                {
                    Data = result,
                };
        }
    }
}