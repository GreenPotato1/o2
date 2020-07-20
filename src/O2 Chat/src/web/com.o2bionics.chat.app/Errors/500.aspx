﻿<%@ Language="c#" %>
<%@ Import Namespace="System.Net" %>
<%@ Import Namespace="Com.O2Bionics.Utils.Web" %>
<%
    Server.ClearError();
    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    Response.TrySkipIisCustomErrors = true;

    if (Request.IsAjaxRequest())
    {
        // mimics Com.O2Bionics.ChatService.Contract.CallResultStatus
        Response.ContentType = "application/json";
        Response.Clear();
        Response.Write(@"{Status:{StatusCode:3,Messages:[Field:'',Message:'Internal error']}}");
        Response.Flush();
        Response.End();
    }
%>
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <meta name="language" content="en">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <link rel="icon" href="data:;base64,=">

    <title>Error page</title>

    <link rel="stylesheet" href="/st/lib/bootstrap/dist/css/bootstrap.min.css">
    <link rel="stylesheet" href="/st/lib/bootstrap-material-design/dist/css/bootstrap-material-design.min.css">
    <link rel="stylesheet" href="/st/lib/bootstrap-material-design/dist/css/ripples.min.css">
    <link rel="stylesheet" href="/st/lib/material-design-icons-iconfont/dist/fonts/material-icons.css">
    <link rel="stylesheet" href="/st/css/style.css">
    <style>
        body { background: #34515e !important; }
    </style>
</head>
<body>
<div class="error-page-message">
    <h1>Oops, something went wrong.</h1>
    <p>Our team was notified and we are working on this issue. Please try again later.</p>
    <i class="material-icons">&#xE002;</i>
</div>
</body>
</html>