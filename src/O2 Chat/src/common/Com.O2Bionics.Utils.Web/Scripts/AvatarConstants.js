var AvatarConstants = AvatarConstants || {};

AvatarConstants.defaultAvatarsBaseUrl = typeof (AvatarConstants.defaultAvatarsBaseUrl) === 'undefined'
                                        ? ''
                                        : AvatarConstants.defaultAvatarsBaseUrl;
AvatarConstants.defaultAvatarsPath = '/st/i/avatars/';

AvatarConstants.defaultAvatarPrefix = 'default/';

AvatarConstants.defaultAvatarName = 'agent.jpg';
AvatarConstants.defaultAvatar = AvatarConstants.defaultAvatarPrefix + AvatarConstants.defaultAvatarName;

AvatarConstants.toAvatarUrl = function (a)
{
  a = a ? a : AvatarConstants.defaultAvatar;
  // string.startsWith() is ES6 method; not availble in ie11
  if (a.lastIndexOf(AvatarConstants.defaultAvatarPrefix) === 0)
    return a.replace(
      AvatarConstants.defaultAvatarPrefix,
      AvatarConstants.defaultAvatarsBaseUrl + AvatarConstants.defaultAvatarsPath);
  else
  {
    console.log('invalid avatar prefix', a);
    return null;
  }
}