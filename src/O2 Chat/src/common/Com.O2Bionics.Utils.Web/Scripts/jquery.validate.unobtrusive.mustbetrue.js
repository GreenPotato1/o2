(function ($)
{
  $.validator.addMethod(
    'mustbetrue',
    function (value, element, param)
    {
      if (!this.depend(param, element))
        return 'dependency-mismatch';
      return element.checked;
    });

  $.validator.unobtrusive.adapters.add(
    'mustbetrue',
    function (options)
    {
      var ruleName = 'mustbetrue';
      options.rules[ruleName] = true;
      if (options.message) options.messages[ruleName] = options.message;
    });
}(jQuery));