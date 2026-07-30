import type { ComponentType, CSSProperties, ReactNode } from 'react';
import type { DisplayContent } from '../displayContent.mjs';
import { createLayoutRegistry } from '../layoutRegistry.mjs';
import ClassicDinerLayout from './ClassicDinerLayout';
import PhotoGridLayout from './PhotoGridLayout';
import './classicDiner.css';
import './photoGrid.css';

export type DisplayLayoutProps = {
  content: DisplayContent;
};

function contrastColor(hexColor: string, dark = '#241B12', light = '#FFFFFF') {
  const match = /^#([0-9A-F]{6})$/i.exec(hexColor);
  if (!match) return light;
  const value = Number.parseInt(match[1], 16);
  const red = (value >> 16) & 255;
  const green = (value >> 8) & 255;
  const blue = value & 255;
  return (red * 299 + green * 587 + blue * 114) / 255000 > 0.55 ? dark : light;
}

type DisplayFrameProps = {
  children: ReactNode;
  content: DisplayContent;
  layoutKey: string;
  requestedLayoutKey: string;
  usedFallback: boolean;
};

export function DisplayFrame({ children, content, layoutKey, requestedLayoutKey, usedFallback }: DisplayFrameProps) {
  const theme = content.theme ?? {
    backgroundColor: '#111315',
    accentColor: '#FFB74D',
    fontFamily: 'Inter'
  };
  const style = {
    '--vennu-background': theme.backgroundColor,
    '--vennu-accent': theme.accentColor,
    '--vennu-font-family': theme.fontFamily,
    '--vennu-foreground': contrastColor(theme.backgroundColor),
    '--vennu-accent-foreground': contrastColor(theme.accentColor),
    fontFamily: theme.fontFamily
  } as CSSProperties;
  return (
    <main
      data-layout={layoutKey}
      data-requested-layout={requestedLayoutKey}
      data-layout-fallback={usedFallback ? 'true' : 'false'}
      style={style}
    >
      {children}
    </main>
  );
}

function DefaultLayout({ content }: DisplayLayoutProps) {
  return (
    <>
      <header>
        <h1>{content.screenName}</h1>
        <p>{content.status}</p>
      </header>
      <dl>
        <dt>Screen key</dt>
        <dd>{content.screenKey}</dd>
        <dt>Layout</dt>
        <dd>{content.layout}</dd>
        <dt>Last seen</dt>
        <dd>{content.lastSeenUtc ?? 'Not yet reported'}</dd>
      </dl>
    </>
  );
}

const layoutRegistry = createLayoutRegistry<ComponentType<DisplayLayoutProps>>([
  {
    key: 'default',
    label: 'Default display',
    renderer: DefaultLayout
  },
  {
    key: 'photo_grid',
    label: 'Photo Grid',
    renderer: PhotoGridLayout
  },
  {
    key: 'classic_diner',
    label: 'Classic Diner',
    renderer: ClassicDinerLayout
  }
]);

export function DisplayLayout({ content }: DisplayLayoutProps) {
  const resolved = layoutRegistry.resolve(content.layout);
  const Layout = resolved.registration.renderer;

  return (
    <DisplayFrame
      content={content}
      layoutKey={resolved.key}
      requestedLayoutKey={resolved.requestedKey}
      usedFallback={resolved.isFallback}
    >
      <Layout content={content} />
    </DisplayFrame>
  );
}
