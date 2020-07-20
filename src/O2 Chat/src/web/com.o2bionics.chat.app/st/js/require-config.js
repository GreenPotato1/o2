require.config({
    baseUrl: '/st/tsjs',
    paths: {
        tslib: '/st/lib/tslib/tslib'
      }
  });

define('lodash',
  function ()
  {
    return _;
  });

define('./typings/moment/moment',
  function ()
  {
    return moment;
  });

define('autolinker',
  function ()
  {
    return Autolinker;
  });
define('chart',
  function ()
  {
    return Chart;
  });