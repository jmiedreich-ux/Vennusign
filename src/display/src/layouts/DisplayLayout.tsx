import { useLayoutEffect, useRef, useState, type ComponentType, type CSSProperties, type ReactNode } from 'react';
import type { DisplayContent } from '../displayContent.mjs';
import { createLayoutRegistry } from '../layoutRegistry.mjs';
import { computeBoardFit, boardFitContainerStyle, type BoardFit } from '../boardFitScale.mjs';
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
import './pageTransition.css';

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

const naturalFit: BoardFit = { scale: 1, width: null };

// Every layout sets `min-height: 100vh` on its own root and nothing else - a floor, not a
// ceiling - so a board taller than the viewport just grows past it with no scroll and no
// indicator (#790: three of six items on a real venue's board were entirely off-screen). This
// measures the actual rendered height against the actual viewport, above every layout rather
// than inside any one of them, and shrinks the whole board uniformly until it fits. It only ever
// shrinks - a board that already fits keeps its natural size - and never below a legibility
// floor, past which some overflow is accepted rather than illegible text.
//
// A uniform scale that only corrects for height also shrinks width by the same factor, wasting
// real width the board never overflowed (#794 - confirmed on real hardware immediately after
// #790 shipped). So when the board doesn't already fit, this also widens the measured container
// before scaling it down, by exactly the amount solveFitWidth computes, so the scaled result
// fills the viewport's width exactly rather than leaving it letterboxed.
function useBoardFitScale() {
  const containerRef = useRef<HTMLDivElement>(null);
  const [fit, setFit] = useState<BoardFit>(naturalFit);
  const appliedRef = useRef<BoardFit>(naturalFit);

  useLayoutEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const applyFit = (next: BoardFit) => {
      const previous = appliedRef.current;
      const widthChanged = Math.abs((next.width ?? -1) - (previous.width ?? -1)) > 0.5;
      const scaleChanged = Math.abs(next.scale - previous.scale) > 0.0005;
      if (!widthChanged && !scaleChanged) {
        // Same result as last time - skip the state update (and the DOM write it causes), or a
        // ResizeObserver observing the very element this resizes would loop reacting to its own
        // no-op mutation.
        return;
      }
      appliedRef.current = next;
      setFit(next);
    };

    const recompute = () => {
      // Release any width this same effect applied on a prior pass first - otherwise this would
      // measure at a width already pinned by that pass, not the board's true natural width.
      container.style.width = '';
      // offsetWidth, not getBoundingClientRect().width - the rect includes this element's own
      // CSS transform, so on every cycle after the first it would read back whatever scale the
      // previous cycle applied instead of the box's true untransformed width, and never converge.
      const naturalWidth = container.offsetWidth;
      const naturalHeight = container.scrollHeight;
      const viewportWidth = window.innerWidth;
      const viewportHeight = window.innerHeight;

      let next: BoardFit;
      if (naturalWidth <= 0 || naturalHeight <= viewportHeight) {
        next = naturalFit;
      } else {
        // One probe at a wider width pins the exact linear relationship between container width
        // and rendered height (see boardFitScale.mjs) - a closed-form solve, not an iterative
        // approximation, so this never needs more than one extra measurement. computeBoardFit
        // decides whether that solve is actually usable (a positive width, and a scale that
        // doesn't need clamping) or whether to fall back to the height-only #790 behavior.
        const probeWidth = naturalWidth * 1.5;
        container.style.width = `${probeWidth}px`;
        const probeHeight = container.scrollHeight;

        next = computeBoardFit(
          { width: naturalWidth, height: naturalHeight },
          { width: probeWidth, height: probeHeight },
          viewportWidth,
          viewportHeight
        );
      }

      // Measuring is destructive - it has to clear and probe container.style.width to read
      // natural values - and applyFit below may legitimately skip its React state update when
      // the result matches what's already applied (the ResizeObserver convergence guard). So the
      // DOM has to be put back in the correct state here, unconditionally, rather than relying on
      // a re-render that might not happen to undo what measuring just did.
      container.style.width = next.width === null ? '' : `${next.width}px`;
      applyFit(next);
    };

    recompute();

    // Older TV WebViews (some Tizen/webOS builds) can lack ResizeObserver entirely. The one-shot
    // recompute() above still fits the board as first rendered on those engines - it just cannot
    // react to a later resize or a font finishing its own load, which is strictly better than the
    // pre-fix behavior of never fitting at all.
    if (typeof ResizeObserver === 'undefined') return;

    const observer = new ResizeObserver(recompute);
    observer.observe(container);
    window.addEventListener('resize', recompute);

    return () => {
      observer.disconnect();
      window.removeEventListener('resize', recompute);
    };
  }, []);

  return { containerRef, scale: fit.scale, width: fit.width };
}

export function DisplayFrame({ children, content, layoutKey, requestedLayoutKey, usedFallback }: DisplayFrameProps) {
  const { containerRef, scale, width } = useBoardFitScale();
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
        data-board-fit-width={width ?? undefined}
        // boardFitContainerStyle picks the transform-origin that matches this fit - see its own
        // comment for why 'top left' and 'top center' are NOT interchangeable here (#802).
        style={boardFitContainerStyle({ scale, width })}
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
  // Changes only when the set of sections actually on screen changes - a real page turn or a
  // virtual sub-page turn - not on every content poll that happens to return the same page, so
  // the fade doesn't retrigger on ordinary refreshes.
  const pageSignature = (content.sections ?? []).map((section) => section.id).join('|');

  return (
    <DisplayFrame
      content={content}
      layoutKey={resolved.key}
      requestedLayoutKey={resolved.requestedKey}
      usedFallback={resolved.isFallback}
    >
      <div className="display-frame__page" key={pageSignature}>
        <Layout content={content} />
      </div>
    </DisplayFrame>
  );
}
