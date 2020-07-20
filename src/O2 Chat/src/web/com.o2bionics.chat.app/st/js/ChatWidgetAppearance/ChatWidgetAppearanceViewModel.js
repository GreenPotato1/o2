'use strict';


function ChatWidgetAppearanceViewModel(
    chatFrameId,
    enabledFeatures,
    themeModel,
    themeMinModel,
    positioningModel,
    minimizedStateTitle,
    hidePoweredBy,
    backgroundPageUrl,
    saveUrl,
    themesUrl)
  {
    var self = this;

    this.chatFrameId = chatFrameId;
    this.saveUrl = saveUrl;
    this.enabledFeatures = enabledFeatures;

    this.themes = ko.observableArray();
    this.themesMin = ko.observableArray();
    this.selectedThemeId = ko.observable(/*themeModel.selectedThemeId*/);
    this.selectedThemeMinId = ko.observable(/*themeMinModel.selectedThemeId*/);
    this.customCssUrl = ko.observable();

    this.locations = ko.observableArray(positioningModel.locations);
    this.selectedLocation = ko.observable(positioningModel.selectedLocation);

    this.offsetX = ko.observable(positioningModel.offsetX);
    this.offsetY = ko.observable(positioningModel.offsetY);

    this.minimizedStateTitle = ko.observable(minimizedStateTitle);

    this.isHidePoweredBy = ko.observable(hidePoweredBy);

    this.isCustomThemeSelectorVisible = ko.pureComputed(
      function () { return self.selectedThemeId() === themeModel.customTheme });
    this.backgroundPageUrl = ko.observable(backgroundPageUrl);

    var promise = getThemes();
    promise.then(function (result) {
            self.themes(result.Maximized);

            if (themeModel.fullCustomizationAllowed) {
                self.themes.push(themeModel.customTheme);
            }
            self.themesMin(result.Minimized);

            self.selectedThemeId(themeModel.selectedThemeId);
            self.selectedThemeMinId(themeMinModel.selectedThemeId);
        },
        function (error) {
            console.log(error);
        });

    this.themeChange = function ()
    {
      var themeId = this.selectedThemeId();
      var theme = themeModel.defaultTheme;

      if (themeId !== themeModel.customTheme)
      {
        theme = $.grep(self.themes(), function (n) { return (n === themeId); })[0];
      }

      this.postMessageToChat({ appearance: { theme: { value: theme } } });
    }

    this.themeMinChange = function ()
    {
      var themeId = this.selectedThemeMinId();
      var theme = $.grep(self.themesMin(), function (n) { return (n === themeId); })[0];

      this.postMessageToChat({ appearance: { themeMin: { value: theme } } });
    }

    this.positioningChange = function ()
    {
      console.log(this.selectedLocation(), this.offsetX(), this.offsetY());
      this.postMessageToChat({
          appearance: {
              positioning: {
                  location: parseInt(this.selectedLocation()),
                  offsetX: this.offsetX(),
                  offsetY: this.offsetY()
                }
            }
        });
    }

    this.minimizedStateTitleChange = function ()
    {
      this.postMessageToChat({
          appearance: {
              minimizedTitleText: {
                  value: this.minimizedStateTitle()
                }
            }
        });
    }

    this.hidePoweredByClick = function ()
    {
      this.postMessageToChat({
          appearance: {
              poweredByVisible: {
                  value: !this.isHidePoweredBy()
                }
            }
        });

      return true;
    }

    this.saveClick = function ()
    {
      var customCssUrl = this.isCustomThemeSelectorVisible() ? this.customCssUrl() : '';

      var data = {
          themeId: this.selectedThemeId(),
          themeMinId: this.selectedThemeMinId(),
          location: this.selectedLocation(),
          offsetX: this.offsetX(),
          offsetY: this.offsetY(),
          minimizedStateTitle: this.minimizedStateTitle(),
          enabledFeatures: this.enabledFeatures,
          customCssUrl: customCssUrl,
          poweredByVisible: !this.isHidePoweredBy()
        };

      console.log(data);

      $.ajax({
            method: 'POST',
            url: this.saveUrl,
            datatype: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(data),
            cache: false
          })
        .done(function (r)
        {
          console.log(r);

          if (r.Status.StatusCode === Enums.CallResultStatus.Success)
            self.toast('Success', 'Saved');
          else if (r.Status.StatusCode === Enums.CallResultStatus.Warning)
            self.toast('Warning', r.Status.Messages[0].Message);
          else
            self.toast('Fail', r.Status.Messages[0].Message);
        })
        .fail(function (r)
        {
          console.log(r);

          self.toast('Fail', 'Save failed');
        });
    }

    this.toast = function (title, messageHtml)
    {
      $.gritter.add({
          title: title,
          text: messageHtml,
          sticky: false,
          time: 5000,
          class_name: 'gritter-custom'
        });
    }

    this.postMessageToChat = function (data)
    {
      var elt = document.getElementById(this.chatFrameId);
      if (elt)
        elt.contentWindow.postMessage(JSON.stringify(data), '*');
    }

    this.changeBackgroundPageUrl = function ()
    {
      var url = self.backgroundPageUrl();

      if (!/^(f|ht)tps?:\/\//i.test(url))
      {
        url = location.protocol + "//" + url;
      }

      self.backgroundPageUrl(url);
    }

    function getThemes() {
        return new Promise(function (resolve, reject) {
            $.ajax({
                url: themesUrl,
                type: 'GET',
                data: { fca: themeModel.fullCustomizationAllowed },
                dataType: 'json',
                success: function (result) {
                    resolve(result);
                },
                error: function (xhr, status, error) {
                    reject(error);
                }
            })
        });
    }
  }