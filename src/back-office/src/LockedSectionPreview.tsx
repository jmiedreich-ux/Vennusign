import { useEffect, useState } from 'react';
import EntitlementLockChip from './EntitlementLockChip';
import { loadMenuEditor, type MenuEditorSnapshot } from './api';
import type { BackOfficeConfiguration } from './config';
import { buildPersonalizedLockedPreview, supportsPersonalizedLockedPreview } from './lockedPreview.mjs';
import type { UpgradeOpportunity } from './upgradeExperience.mjs';
import VennusignLoader from "./VennusignLoader";

type Props = {
  opportunity: Readonly<UpgradeOpportunity>;
  configuration: BackOfficeConfiguration;
  accessToken: string;
  venueId: string;
  venueName: string;
  onDismiss: (featureKey: string) => void;
  onUpgrade: (opportunity: Readonly<UpgradeOpportunity>) => void;
};

const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });

export default function LockedSectionPreview({ opportunity, configuration, accessToken, venueId, venueName, onDismiss, onUpgrade }: Props) {
  const personalized = supportsPersonalizedLockedPreview(opportunity.featureKey);
  const [snapshot, setSnapshot] = useState<MenuEditorSnapshot>();
  const [previewState, setPreviewState] = useState<'loading' | 'ready' | 'error'>(personalized ? 'loading' : 'ready');

  useEffect(() => {
    if (!personalized) return;
    let current = true;
    setPreviewState('loading');
    setSnapshot(undefined);
    loadMenuEditor(configuration, accessToken, venueId)
      .then(value => { if (current) { setSnapshot(value); setPreviewState('ready'); } })
      .catch(() => { if (current) { setSnapshot(undefined); setPreviewState('error'); } });
    return () => { current = false; };
  }, [accessToken, configuration, personalized, venueId]);

  const preview = snapshot ? buildPersonalizedLockedPreview(snapshot) : undefined;

  return (
    <section className="locked-section-preview" data-testid="locked-preview" data-feature={opportunity.featureKey} aria-labelledby={`locked-${opportunity.featureKey}`}>
      {personalized ? <div className="locked-section-glimpse personalized-locked-preview" aria-label={`Preview using ${venueName} menu content`}>
        {previewState === 'loading' ? <VennusignLoader message="Loading your venue preview…" />
          : previewState === 'error' ? <div className="personalized-locked-preview__state" role="status"><strong>Preview unavailable</strong><span>Your content is unchanged. Open this page again to retry.</span></div>
          : !preview || preview.sections.length === 0 ? <div className="personalized-locked-preview__state"><strong>{venueName}</strong><span>Add active menu items to personalize this preview.</span></div>
          : <>
            <header><div><small>Your content · preview only</small><strong>{preview.menuName}</strong></div>{preview.dailySpecial ? <span>{preview.dailySpecial}</span> : null}</header>
            <div className="personalized-locked-preview__sections">
              {preview.sections.map(section => <section key={section.id} aria-label={section.name}>
                <h4>{section.name}</h4>
                {section.items.map(item => <div className={item.available ? '' : 'unavailable'} key={`${section.id}-${item.name}`}>
                  <span>{item.name}</span><span>{item.available ? currency.format(item.price) : 'Sold out'}</span>
                </div>)}
              </section>)}
            </div>
          </>}
      </div> : <div className="locked-section-glimpse" aria-hidden="true"><span /><span /><span /></div>}
      <div className="locked-section-copy">
        <EntitlementLockChip opportunity={opportunity} onOpen={onUpgrade} />
        <h3 id={`locked-${opportunity.featureKey}`}>{opportunity.title}</h3>
        <p>{opportunity.benefit}</p>
        <div>
          <button className="quiet" type="button" onClick={() => onDismiss(opportunity.featureKey)}>Not now</button>
        </div>
      </div>
    </section>
  );
}
