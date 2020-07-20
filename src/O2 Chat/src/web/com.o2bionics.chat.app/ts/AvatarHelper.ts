
export default class AvatarHelper {

  private static defaultAvatarsBaseUrl = '';
  private static defaultAvatarsPath = '/st/i/avatars/';

  private static defaultAvatarPrefix = 'default/';

  private static defaultAvatarName = 'agent.jpg';
  private static defaultAvatar = AvatarHelper.defaultAvatarPrefix + AvatarHelper.defaultAvatarName;

  public static toAvatarUrl(a: string | null): string | null
  {
    a = a ? a : AvatarHelper.defaultAvatar;
    // string.startsWith() is ES6 method; not availble in ie11
    if (a.lastIndexOf(AvatarHelper.defaultAvatarPrefix) === 0)
      return a.replace(
        AvatarHelper.defaultAvatarPrefix,
        AvatarHelper.defaultAvatarsBaseUrl + AvatarHelper.defaultAvatarsPath);
    else
    {
      console.log('invalid avatar prefix', a);
      return null;
    }
  }
}