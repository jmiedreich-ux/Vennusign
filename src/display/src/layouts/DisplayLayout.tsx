import { useLayoutEffect, useRef, useState, type ComponentType, type CSSProperties, type ReactNode } from 'react';
import type { DisplayContent } from '../displayContent.mjs';
import { createLayoutRegistry } from '../layoutRegistry.mjs';
import { computeFitScale } from '../boardFitScale.mjs';
import ClassicDinerLayout from './ClassicDinerLayout';
import DailySpecialHeroLayout from './DailySpecialHeroLayout';
import NeonChalkboardLayout from './NeonChalkboardLayout';
import PhotoGridLayout from './PhotoGridLayout';
import SplitLayout from './SplitLayout';
import HappyHourBanner from './HappyHourBanner';
import PromotionBanner from './PromotionBanner';
import ClassicChalkboardLayout from './ClassicChalkboardLayout';
import TapStripsLayout from './TapStripsLayout';
import DigitalTapBoardLayout from './DigitalTapBoardLayout';
import './classicDiner.css';
import './dailySpecialHero.css';
import './neonChalkboard.css';
import './photoGrid.css';
import './splitLayout.css';
import './happyHour.css';
import './promotion.css';
import './classicChalkboard.css';
import './tapStrips.css';
import './digitalTapBoard.css';

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

// Every layout sets `min-height: 100vh` on its own root and nothing else - a floor, not a
// ceiling - so a board taller than the viewport just grows past it with no scroll and no
// indicator (#790: three of six items on a real venue's board were entirely off-screen). This
// measures the actual rendered height against the actual viewport, above every layout rather
// than inside any one of them, and shrinks the whole board uniformly until it fits. It only ever
// shrinks - a board that already fits keeps its natural size - and never below a legibility
// floor, past which some overflow is accepted rather than illegible text.
function useBoardFitScale() {
  const containerRef = useRef<HTMLDivElement>(null);
  const [scale, setScale] = useState(1);

  useLayoutEffect(() => {
    const container = containerRef.current;
    if (!container || typeof ResizeObserver === 'undefined') return;

    const recompute = () => setScale(computeFitScale(container.scrollHeight, window.innerHeight));
    recompute();

    const observer = new ResizeObserver(recompute);
    observer.observe(container);
    window.addEventListener('resize', recompute);

    return () => {
      observer.disconnect();
      window.removeEventListener('resize', recompute);
    };
  }, []);

  return { containerRef, scale };
}

export function DisplayFrame({ children, content, layoutKey, requestedLayoutKey, usedFallback }: DisplayFrameProps) {
  const { containerRef, scale } = useBoardFitScale();
  const theme = content.theme ?? {
    backgroundColor: '#111315',
    accentColor: '#FFB74D',
    fontFamily: 'Inter',
    presetKey: 'bar_classic',
    titleColor: '#F8F5E9',
    glowColor: '#00E5FF',
    boardBackgroundColor: '#071013',
    sectionColors: ['#00E5FF', '#FF2BD6', '#FFE66D', '#7CFF6B'],
    glowIntensity: 1,
    titleFont: 'Righteous',
    itemFont: 'Caveat'
  };
  const style = {
    '--vennu-background': theme.backgroundColor,
    '--vennu-accent': theme.accentColor,
    '--vennu-font-family': theme.fontFamily,
    '--vennu-foreground': contrastColor(theme.backgroundColor),
    '--vennu-accent-foreground': contrastColor(theme.accentColor),
    '--vennu-title-color': theme.titleColor,
    '--vennu-glow-color': theme.glowColor,
    '--vennu-board-background': theme.boardBackgroundColor,
    '--vennu-section-color-1': theme.sectionColors[0] ?? theme.glowColor,
    '--vennu-section-color-2': theme.sectionColors[1] ?? theme.glowColor,
    '--vennu-section-color-3': theme.sectionColors[2] ?? theme.glowColor,
    '--vennu-section-color-4': theme.sectionColors[3] ?? theme.glowColor,
    '--vennu-glow-intensity': theme.glowIntensity,
    '--vennu-frame-glow': `${0.7 * theme.glowIntensity}rem`,
    '--vennu-title-glow': `${0.55 * theme.glowIntensity}rem`,
    '--vennu-section-glow': `${0.35 * theme.glowIntensity}rem`,
    '--vennu-title-font': theme.titleFont,
    '--vennu-item-font': theme.itemFont,
    fontFamily: theme.fontFamily
  } as CSSProperties;
  return (
    <main
      data-layout={layoutKey}
      data-requested-layout={requestedLayoutKey}
      data-layout-fallback={usedFallback ? 'true' : 'false'}
      style={style}
    >
      <HappyHourBanner content={content} />
      <PromotionBanner content={content} />
      <div
        ref={containerRef}
        data-board-fit-scale={scale}
        style={{ transform: `scale(${scale})`, transformOrigin: 'top center' }}
      >
        {children}
      </div>
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
  },
  {
    key: 'neon_chalkboard',
    label: 'Neon Chalkboard',
    renderer: NeonChalkboardLayout
  },
  {
    key: 'split_layout',
    label: 'Split Layout',
    renderer: SplitLayout
  },
  {
    key: 'daily_special_hero',
    label: 'Daily Special Hero',
    renderer: DailySpecialHeroLayout
  },
  {
    key: 'classic_chalkboard',
    label: 'Classic Chalkboard Drinks',
    renderer: ClassicChalkboardLayout
  },
  {
    key: 'tap_strips',
    label: 'Tap Strips',
    renderer: TapStripsLayout
  },
  {
    key: 'digital_tap_board',
    label: 'Digital Tap Board',
    renderer: DigitalTapBoardLayout
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
