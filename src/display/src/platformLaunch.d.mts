export type TvPlatform = 'android_tv' | 'fire_tv' | 'tizen' | 'webos';
export type PlatformBridge = { platform?: string; appVersion?: string; screenId?: string };
export type PlatformLaunch = { platform: TvPlatform | 'browser'; appVersion: string; pathname: string };
export const supportedTvPlatforms: readonly TvPlatform[];
export function resolvePlatformLaunch(pathname: string, bridge?: PlatformBridge): PlatformLaunch;
