import type { ComponentType, ReactNode } from 'react';
import type { DisplayContent } from '../displayContent.mjs';
import { createLayoutRegistry } from '../layoutRegistry.mjs';
import ClassicDinerLayout from './ClassicDinerLayout';
import PhotoGridLayout from './PhotoGridLayout';
import './classicDiner.css';
import './photoGrid.css';

export type DisplayLayoutProps = {
  content: DisplayContent;
};

type DisplayFrameProps = {
  children: ReactNode;
  layoutKey: string;
  requestedLayoutKey: string;
  usedFallback: boolean;
};

export function DisplayFrame({ children, layoutKey, requestedLayoutKey, usedFallback }: DisplayFrameProps) {
  return (
    <main
      data-layout={layoutKey}
      data-requested-layout={requestedLayoutKey}
      data-layout-fallback={usedFallback ? 'true' : 'false'}
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
      layoutKey={resolved.key}
      requestedLayoutKey={resolved.requestedKey}
      usedFallback={resolved.isFallback}
    >
      <Layout content={content} />
    </DisplayFrame>
  );
}
