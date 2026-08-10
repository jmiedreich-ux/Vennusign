import { useCallback, useEffect, useRef, useState } from "react";
import { BoardRenderer } from "./BoardRenderer";
import type { BoardRendererProps } from "./BoardRenderer";
import { boardLogicalHeight, boardLogicalWidth, scaleToFit } from "./boardScale.mjs";

export type BoardFrameProps = BoardRendererProps & {
  /**
   * Drawn instead of the board when there is nothing published yet. A menu that
   * has never been published has no board to picture, and inventing one would be
   * a lie about what a screen is showing.
   */
  fallback?: React.ReactNode;
};

/**
 * A board at one fixed logical size, scaled to whatever box it is given.
 *
 * This is the decision that makes a shelf card honest. The board is always laid
 * out at 1920x1080 and then scaled; a card, the builder canvas, Play and the real
 * TV are the SAME DOM at different scales, never a per-size re-layout. A card
 * that laid itself out independently would drift from the TV, and nobody would
 * notice until a guest did.
 *
 * Content taller than the board is clipped. Pages as a consequence of overflow
 * are milestone 5 and the dwell cycle is milestone 4; a fitting model invented
 * here would be a second layout the player then has to unlearn. Clipping is what
 * an unpaginated TV does, and it is honest about it.
 */
export function BoardFrame({ fallback, ...board }: BoardFrameProps) {
  const container = useRef<HTMLDivElement>(null);
  const [scale, setScale] = useState(1);

  const measure = useCallback(() => {
    const element = container.current;
    if (!element) return;
    setScale(scaleToFit(element.clientWidth, element.clientHeight));
  }, []);

  useEffect(() => {
    measure();

    const element = container.current;
    if (!element || typeof ResizeObserver === "undefined") return;

    const observer = new ResizeObserver(measure);
    observer.observe(element);
    return () => observer.disconnect();
  }, [measure]);

  const hasBoard = (board.board?.sections?.length ?? 0) > 0;

  return (
    <div className="board-frame" ref={container} data-testid="board-frame">
      {hasBoard ? (
        <div
          className="board-frame-stage"
          style={{
            width: `${boardLogicalWidth}px`,
            height: `${boardLogicalHeight}px`,
            transform: `scale(${scale})`
          }}
        >
          <BoardRenderer {...board} />
        </div>
      ) : (
        <div className="board-frame-empty" data-testid="board-frame-empty">
          {fallback}
        </div>
      )}
    </div>
  );
}
