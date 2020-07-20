window.O2Bionics = window.O2Bionics || {};

(function (window)
{
  // read-only
  var document = window.document;
  var widgetIframeElement = null;

  window.O2Bionics.postError = function (error, extraMessage) {

    //Begin. copied from ErrorLogger.js
    function getStr(value)
    {
      return ('string' === typeof(value) && null != value && 0 < value.length) ? value : null;
    }
    function getErrorMessage(messageOrEvent, error)
    {
      var m = getStr(messageOrEvent);
      if (null != m)
        return m;

      if (null != error)
      {
        m = getStr(error.message);
        if (null != m)
          return m;
      }

      if (null != messageOrEvent && 'function' === typeof(messageOrEvent.toString))
      {
        m = getStr(messageOrEvent.toString());
      }
      return m;
    }
    //End. copied from ErrorLogger.js

    try {
      if (null == widgetIframeElement || null == widgetIframeElement.contentWindow) {
        console.debug("The widgetIframeElement is not initialized yet.");
        return;
      }
      var m = getErrorMessage(error, error);
      if (null == error || null == m || 0 === m.length) {
        console.debug("No message in postError.");
        return;
      }

      var errorInfo = { 'Message': extraMessage, 'ExceptionMessage' : m };
      var loc = document.location;
      if (null != loc && null != loc.href)
        errorInfo.Url = loc.href;

      if (null != error.stack && 'function' === typeof (error.stack.toString) )
        errorInfo.ExceptionStack = error.stack.toString();

      var message = '{"error": ' + JSON.stringify(errorInfo) + '}';
      widgetIframeElement.contentWindow.postMessage(message, '*');
    }
    catch (e) {
      console.error(e);
    }
  }

  try {
    // constants
    var o2bionicsChatFuncName = 'O2WebChat';
    var chatDomainOriginRe = /^https?:\/\/.+\.o2bionics\.com(:\d+)?$/i;
    var identifierBase = 'com-o2bionics-webchat-';
    var storageKey = identifierBase + 'position-';
    var iframeIdentifier = identifierBase + 'iframe';
    var dragOverlayIdentifier = identifierBase + 'overlay';
    var scriptIdentifier = identifierBase + 'script';
    var widgetContainerId = identifierBase + 'container';
    var adminMenuHeightPx = 62;

    // read-only
    var customerParams = getPageParams();
    var isDemo = document.getElementById(widgetContainerId) ? true : false;
    var widgetParentElement = null;
    var widgetIframeOverlayElement = null;

    // can be changed
    var isMinimized = true;
    var defaultWidgetPosition = null;
    var savedWidgetPosition = loadWidgetPosition();

    if (customerParams)
      getWidgetParentElement(function (elt) {
        widgetParentElement = elt;

        on('message', handleWindowMessage);

        createWidgetElements(document, widgetParentElement, customerParams.cid);
      });


    function getPageParams() {
      var func = window[o2bionicsChatFuncName] || null;
      if (func == null) return null;
      var paramsArray = func.p || null;
      if (paramsArray == null) return null;
      var params = paramsArray.pop() || null;
      if (params == null) return null;

      params.pageUrl = document.location.href;
      return params;
    };


    function isOutsideScreen(screenPos, width, height) {
      var innerHeight = window.innerHeight;
      var top = screenPos.top !== null ? screenPos.top : (innerHeight - screenPos.bottom - height);
      if (top < 0) return true;
      var bottom = screenPos.bottom !== null ? (innerHeight - screenPos.bottom) : (screenPos.top + height);
      if (bottom > innerHeight) return true;

      var innerWidth = window.innerWidth;
      var left = screenPos.left !== null ? screenPos.left : (innerWidth - screenPos.right - width);
      if (left < 0) return true;
      var right = screenPos.right !== null ? (innerWidth - screenPos.right) : (screenPos.left + width);
      if (right > innerWidth) return true;

      return false;
    }

  function calculatePositionCoordinates(pos, iframe) {
      var left = null;
      var right = null;
      var top = null;
      var bottom = null;

      switch (pos.location) {
        case 1:
          // Enums.ChatWidgetLocation.TopLeft
      left = iframe.parentElement.offsetLeft + pos.offsetX;
          top = pos.offsetY + (isDemo ? adminMenuHeightPx : 0);
          break;

        case 2:
          // Enums.ChatWidgetLocation.TopRight
          right = pos.offsetX;
          top = pos.offsetY + (isDemo ? adminMenuHeightPx : 0);
          break;
        case 3:
          // Enums.ChatWidgetLocation.BottomLeft
      	  left = iframe.parentElement.offsetLeft + pos.offsetX;
          bottom = pos.offsetY;
          break;
        case 4:
          // Enums.ChatWidgetLocation.BottomRight
          right = pos.offsetX;
          bottom = pos.offsetY;
          break;
      }

      return { left: left, top: top, right: right, bottom: bottom };
    }

    function setWidgetPosition() {
      var iframe = widgetIframeElement;

      var width = parseInt(iframe.style.width.trim('px'), 10);
      var height = parseInt(iframe.style.height.trim('px'), 10);

      var pos = defaultWidgetPosition;
      if (!isMinimized && savedWidgetPosition) pos = savedWidgetPosition;

      var screenPos = calculatePositionCoordinates(pos, iframe);
      if (isOutsideScreen(screenPos, width, height) && pos !== defaultWidgetPosition)
        screenPos = calculatePositionCoordinates(defaultWidgetPosition, iframe);

      iframe.style.left = screenPos.left !== null ? (screenPos.left + 'px') : 'auto';
      iframe.style.top = screenPos.top !== null ? (screenPos.top + 'px') : 'auto';
      iframe.style.right = screenPos.right !== null ? (screenPos.right + 'px') : 'auto';
      iframe.style.bottom = screenPos.bottom !== null ? (screenPos.bottom + 'px') : 'auto';

      adjustOverlayPosition(iframe);
    }

    function adjustOverlayPosition(iframe) {
      var overlay = widgetIframeOverlayElement;

      overlay.style.left = iframe.style.left;
      overlay.style.top = iframe.style.top;
      var iframeRight = parseInt(iframe.style.right.trim('px'), 10);
      overlay.style.right = (iframe.clientWidth - overlay.clientWidth + iframeRight) + 'px';
      var iframeBottom = parseInt(iframe.style.bottom.trim('px'), 10);
      overlay.style.bottom = (iframe.clientHeight - overlay.clientHeight + iframeBottom) + 'px';
    }

    function draggable(iframe) {
      var overlay = widgetIframeOverlayElement;

      // disable dragging if the widget is minimized
      if (isMinimized || isDemo) {
        overlay.style.zIndex = iframe.style.zIndex - 1;
        return;
      }

      overlay.style.zIndex = iframe.style.zIndex + 1;

      var prnt = widgetParentElement;

      adjustOverlayPosition(iframe);

      overlay.onmousedown = function (e) {
        var oldmovehandler;
        var olduphandler;

        var coords = getCoords(overlay);

        var shiftX = e.pageX - coords.left;
        var shiftY = e.pageY - coords.top;

        var initX = e.pageX;
        var initY = e.pageY;

        var isDrag = false;

        var attachOffset = 20;

        var oldOverlayHeight = overlay.style.height;

        overlay.style.left = coords.left + 'px';
        overlay.style.top = coords.top + 'px';
        overlay.style.height = iframe.style.height;

        if (document.addEventListener) {
          document.addEventListener('mousemove', moveHandler, true);
          document.addEventListener('mouseup', upHandler, true);
        }
        else if (document.attachEvent) {
          overlay.setCapture();
          overlay.attachEvent('onmousemove', moveHandler);
          overlay.attachEvent('onmouseup', upHandler);
          overlay.attachEvent('onlosecapture', upHandler);
        }
        else {
          oldmovehandler = document.onmousemove;
          olduphandler = document.onmouseup;
          document.onmousemove = moveHandler;
          document.onmouseup = upHandler;
        }

        if (e.stopPropagation) e.stopPropagation();
        else e.cancelBubble = true;

        if (e.preventDefault) e.preventDefault();
        else e.returnValue = false;

        var windowInnerWidth = window.innerWidth;
        var windowInnerHeight = window.innerHeight;

        var hasVertScrollbar = windowInnerWidth > document.documentElement.clientWidth;
        var hasHorizScrollbar = windowInnerHeight > document.documentElement.clientHeight;

        var scrollWidth = hasVertScrollbar ? 17 : 0;
        var scrollHeight = hasHorizScrollbar ? 17 : 0;

        function moveAt(e) {
          var left = e.pageX - shiftX;
          var top = e.pageY - shiftY;

          var deltaX = e.pageX - initX;
          var deltaY = e.pageY - initY;

          var prntOffsetLeft = prnt.offsetLeft;
          var prntOffsetTop = prnt.offsetTop;
          var iframeScrollWidth = iframe.scrollWidth;
          var iframeScrollHeight = iframe.scrollHeight;

          if (left >= prntOffsetLeft && (left + iframeScrollWidth) <= windowInnerWidth - scrollWidth)
            overlay.style.left = left + 'px';

          if (left <= attachOffset && deltaX < 0)
            overlay.style.left = prntOffsetLeft + 'px';

          if ((left + iframeScrollWidth) >= (windowInnerWidth - scrollWidth - attachOffset) && deltaX > 0)
            overlay.style.left = (windowInnerWidth - scrollWidth - iframeScrollWidth) + 'px';

          if (top >= prntOffsetTop && (top + iframeScrollHeight) <= windowInnerHeight - scrollHeight)
            overlay.style.top = top + 'px';

          if (top <= attachOffset && deltaY < 0)
            overlay.style.top = prntOffsetTop + 'px';

          if ((top + iframeScrollHeight) >= (windowInnerHeight - scrollHeight - attachOffset) && deltaY > 0)
            overlay.style.top = (windowInnerHeight - scrollHeight - iframeScrollHeight) + 'px';

          iframe.style.left = overlay.style.left;
          iframe.style.top = overlay.style.top;
        }

        function moveHandler(e) {
          if (!e) e = window.event;

          isDrag = true;
          moveAt(e);

          if (e.stopPropagation) e.stopPropagation();
          else e.cancelBubble = true;
        }


        function upHandler(e) {
          if (!e) e = window.event;

          if (document.removeEventListener) {
            document.removeEventListener('mouseup', upHandler, true);
            document.removeEventListener('mousemove', moveHandler, true);
          }
          else if (document.detachEvent) {
            overlay.detachEvent('onlosecapture', upHandler);
            overlay.detachEvent('onmouseup', upHandler);
            overlay.detachEvent('onmousemove', moveHandler);
            overlay.releaseCapture();
          }
          else {
            document.onmouseup = olduphandler;
            document.onmousemove = oldmovehandler;
          }

          overlay.style.height = oldOverlayHeight;

          if (!isDrag) {
            postHeaderClickMessage();
          }
          else {
            var pos = {
              location: 1,
              offsetX: iframe.offsetLeft,
              offsetY: iframe.offsetTop
            };
            saveWidgetPosition(pos);
          }

          if (e.stopPropagation) e.stopPropagation();
          else e.cancelBubble = true;
        }
      }

      function getCoords(elem) {
        var top = elem.offsetTop;
        var left = elem.offsetLeft;

        return {
          top: top,
          left: left
        };
      }
    }


    function getWidgetPositionStorageKey() {
      return storageKey + '-' + customerParams.cid;
    }

    function loadWidgetPosition() {
      if (isDemo) return null;
      if (typeof (localStorage) === 'undefined') return null;

      var s = localStorage.getItem(getWidgetPositionStorageKey());
      if (!s) return null;
      var pp = s.split(',');
      if (pp.length !== 3) return null;
      return {
        location: parseInt(pp[0], 10),
        offsetX: parseInt(pp[1], 10),
        offsetY: parseInt(pp[2], 10)
      };
    }

    function saveWidgetPosition(pos) {
      savedWidgetPosition = pos;

      if (typeof (localStorage) === 'undefined') return;

      var pp = '' + pos.location + ',' + pos.offsetX + ',' + pos.offsetY;
      localStorage.setItem(getWidgetPositionStorageKey(), pp);
    }

    function createWidgetElements(doc, widgetParentElement, cid) {
      var iframe = doc.createElement('iframe');
      iframe.id = iframeIdentifier;
      iframe.src = getScriptUriBase(doc, scriptIdentifier) + '/Client/chatframe.cshtml?cid=' + encodeURIComponent(cid);
      if (isDemo)
        iframe.src += '&m=demo';
      iframe.scrolling = 'no';
      iframe.frameBorder = '0';
      iframe.style.visibility = 'hidden';
      iframe.style.width = '280px';
      iframe.style.height = '37px'; //original value 50px
      iframe.style.zIndex = 30000;
      iframe.style.position = 'fixed';
      iframe.style.bottom = '0';
      iframe.style.right = '15px';
      iframe.style.borderWidth = '0';
      iframe.style.borderStyle = 'solid';
      widgetParentElement.appendChild(iframe);

      var overlay = doc.createElement('div');
      overlay.id = dragOverlayIdentifier;
      overlay.style.cursor = 'move';
      overlay.style.position = 'fixed';
      overlay.style.left = iframe.style.left;
      overlay.style.top = iframe.style.top;
      overlay.style.right = iframe.style.right;
      overlay.style.bottom = iframe.style.bottom;
      overlay.style.zIndex = iframe.style.zIndex + 1;
      overlay.style.width = '215px'; //iframe.style.width;
      overlay.style.height = '37px';
      widgetParentElement.appendChild(overlay);

      widgetIframeElement = iframe;
      widgetIframeOverlayElement = overlay;
    }

    function setWidgetState(width, height, minimized) {
      isMinimized = minimized;

      var iframe = widgetIframeElement;
      iframe.style.width = width + 'px';
      iframe.style.height = height + 'px';
      console.log(height);
      iframe.style.visibility = 'visible';

      draggable(iframe);

      if (!isMinimized)
        setWidgetPosition();
    };

    function updateWidgetDefaultPosition(position) {
      defaultWidgetPosition = position;

      setWidgetPosition();
    }

    function postParamsMessage(visitorExternalId, pageUrl, customText) {
      var ppt = '';
      ppt = jsonAddString(ppt, 'visitorExternalId', visitorExternalId);
      ppt = jsonAddString(ppt, 'pageUrl', pageUrl);
      ppt = jsonAddString(ppt, 'customText', customText);

      widgetIframeElement.contentWindow.postMessage('{"params": {' + ppt + '}}', '*');
    }

    function postHeaderClickMessage() {
      widgetIframeElement.contentWindow.postMessage('{"hdrclck":{}}', '*');
    }

    function handleWindowMessage(event) {
      try
      {
        if (!chatDomainOriginRe.test(event.origin)) return;
        if (event.data == null) return;

        // unfortunately ie 8 can't pass objects in event.data, only strings are supported.

        var data = event.data.split(',');
        if (data.length < 1) return;
        var messageType = data[0];
        switch (messageType)
        {
          // ['show',width,height,isMinimized(0|1)] --> sets iframe dimensions, shows iframe
        case 'show':
          if (data.length < 4) return;

          var width = parseInt(data[1], 10);
          var height = parseInt(data[2], 10);
          var minimized = data[3] === '1';

          setWidgetState(width, height, minimized);
          break;

        // ['get-parameters'] --> posts an message with the page supported parameters
        case 'get-parameters':

          postParamsMessage(
            customerParams.visitorExternalId,
            customerParams.pageUrl,
            customerParams.customText);
          break;

        // ['set-default-position',location,offsetX,offsetY] --> moves iframe to the provided position
        case 'set-default-position':
          if (data.length < 4) return;

          var position = {
              location: parseInt(data[1], 10),
              offsetX: parseInt(data[2], 10),
              offsetY: parseInt(data[3], 10)
            };

          updateWidgetDefaultPosition(position);
          break;
        }
      }
      catch (e)
      {
        window.O2Bionics.postError(e, 'handleWindowMessage');
      }
    }


    // tools

    function on(eventName, eventFunc) {
      if (window.addEventListener)
        addEventListener(eventName, eventFunc, false);
      else
        attachEvent('on' + eventName, eventFunc);
    }

    function jsonAddString(accum, name, s) {
      if (s) {
        var r = accum;
        if (accum.length > 0) r += ',';
        return r + '"' + name + '":"' + s.replace('\\', '\\\\').replace('"', '\\"') + '"';
      }
      else return accum;
    }

    function getWidgetParentElement(callback) {
      /// <signature><param name="callback(parentElement)" type="Function">Callback.</param></signature>

      var widgetContainer = document.getElementById(widgetContainerId);
      if (widgetContainer)
        callback(widgetContainer);
      else
        getDocumentBody(callback);

      function getDocumentBody(callback) {
        // see http://stackoverflow.com/questions/9916747/why-is-document-body-null-in-my-javascript
        if (document.body) {
          callback(document.body);
          return;
        }

        setTimeout(function ()
        {
          //chat frame won't be available.
          getDocumentBody(callback);
        }, 5);
      }
    }

    function getScriptUriBase(doc, id) {
      var scriptUrl = doc.getElementById(id).src;
      var arr = scriptUrl.split('?')[0].split('/');
      return arr[0] + '//' + arr[2];
    }
  }
  catch (e) {
    window.O2Bionics.postError(e, 'bootchat');
  }
})(window);