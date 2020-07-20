using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Threading;

namespace Com.O2Bionics.Utils.Web
{
    public static class Captcha
    {
        private const string CaptchaEncryptKeySettingsName = "CaptchaEncryptPassPhrase";

        private const string AllowedCaptchaCharacters = "123456789ABCDEFGHIJKLMNPRSTUVWXYZ";
        private const int CharactersNumber = 4;


        private const int ImageWidth = 80;
        private const int ImageHeight = 30;
        private const float FontSize = ImageHeight * 0.8f;
        private const float CharacterWidth = (float)ImageWidth / CharactersNumber;

        private static readonly Color m_foreColor = Color.BlueViolet;
        private static readonly Color m_backColor = Color.Chartreuse;

        private static readonly HatchStyle[] m_hatchStyles =
            {
                HatchStyle.DashedVertical,
                HatchStyle.ZigZag,
                HatchStyle.Wave,
                HatchStyle.HorizontalBrick,
                HatchStyle.DottedGrid,
                HatchStyle.SmallGrid,
                HatchStyle.SmallCheckerBoard,
                HatchStyle.OutlinedDiamond,
            };


        private static readonly string m_captchaEncryptKey = GetCaptchaEncryptKey();


        public static string CreateHash()
        {
            var ticks = Environment.TickCount;
            var text = ticks + "|" + RandomString(CharactersNumber);
            return StringEncryptor.Encrypt(text, m_captchaEncryptKey);
        }

        private static int m_seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> m_random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref m_seed)), false);

        private static string RandomString(int n)
        {
            var chars = new char[n];
            for (var i = 0; i < n; i++)
                chars[i] = AllowedCaptchaCharacters[m_random.Value.Next(AllowedCaptchaCharacters.Length)];
            return new string(chars);
        }

        public static byte[] CreatePngImage(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new Exception("Hash can't be null or Whitespace");
            var hashText = StringEncryptor.Decrypt(hash, m_captchaEncryptKey);
            var parts = hashText.Split('|');
            if (parts.Length != 2)
                throw new Exception("Invalid Hash structure");
            var text = parts[1];
            if (text == null || text.Length != CharactersNumber)
                throw new Exception("Invalid Hash structure");

            Debug.WriteLine("CAPTCHA image for " + text);

            return Draw(text);
        }

        public static bool IsValidResponse(string hash, string text)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new Exception("CAPTCHA Hash can't be null or Whitespace");
            if (string.IsNullOrWhiteSpace(text))
                throw new Exception("CAPTCHA Text can't be null or Whitespace");
            var decryptedHash = StringEncryptor.Decrypt(hash, m_captchaEncryptKey);
            var decryptedParts = decryptedHash.Split('|');
            if (decryptedParts.Length < 2)
            {
                Debug.WriteLine("CAPTCHA: invalid decrypted hash: '" + decryptedHash + "'");
                return false;
            }
            var hashText = decryptedParts[1];
            Debug.WriteLine("CAPTCHA: hash text: '{0}', provided text: '{1}'", hashText, text);
            return StringComparer.OrdinalIgnoreCase.Equals(text, hashText);
        }

        private static byte[] Draw(string text)
        {
            using (var bitmap = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format24bppRgb))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                var random = m_random.Value;

                var hatchStyle = m_hatchStyles[random.Next(m_hatchStyles.Length)];
                using (var brush = new HatchBrush(hatchStyle, m_foreColor, m_backColor))
                    graphics.FillRectangle(brush, new RectangleF(0, 0, ImageWidth, ImageHeight));

                using (var font = new Font(FontFamily.GenericMonospace, FontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                using (var brush = new SolidBrush(m_foreColor))
                using (var rotate = new Matrix())
                    for (var i = 0; i < text.Length; i++)
                    {
                        var rotateAngle = random.Next(-40, 40);
                        var xShift = ((float)random.NextDouble() * 0.2f - 0.1f) * CharacterWidth;
                        var yShift = ((float)random.NextDouble() * 0.3f - 0.1f) * ImageHeight;
                        rotate.Reset();
                        rotate.RotateAt(rotateAngle, new PointF(CharacterWidth * (i + 0.5f), ImageHeight / 2f));
                        graphics.Transform = rotate;
                        graphics.DrawString(text.Substring(i, 1), font, brush, xShift + CharacterWidth * i, yShift);
                    }

                var stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private static string GetCaptchaEncryptKey()
        {
            var key = ConfigurationManager.AppSettings[CaptchaEncryptKeySettingsName];
            if (string.IsNullOrWhiteSpace(key))
            {
                const string message =
                    "Non empty and non whitespace password should be specified in appSettings["
                    + CaptchaEncryptKeySettingsName
                    + "]";
                throw new Exception(message);
            }
            return key;
        }
    }
}