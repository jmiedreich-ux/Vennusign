import type { CreateVenueRequest } from "./api";

export type VenueDraft = CreateVenueRequest;
export type VenueDraftValidation = {
  venue: CreateVenueRequest;
  errors: Partial<Record<keyof CreateVenueRequest, string>>;
  valid: boolean;
};

export function validateVenueDraft(draft: Partial<VenueDraft>): VenueDraftValidation;
