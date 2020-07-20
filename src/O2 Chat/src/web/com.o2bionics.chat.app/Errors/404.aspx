﻿<%@ Language="c#" %>
<%@ Import Namespace="System.Net" %>
<%
    Server.ClearError();
    Response.StatusCode = (int)HttpStatusCode.NotFound;
    Response.TrySkipIisCustomErrors = true;
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
    <h1>Oops, page not found.</h1>
    <span class="page-not-found-text">404</span>
</div>
</body>
</html>