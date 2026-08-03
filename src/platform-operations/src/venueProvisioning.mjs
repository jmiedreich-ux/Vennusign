const limits = {
  name: 200,
  timezone: 100,
  type: 50,
  primaryLanguage: 10,
  secondaryLanguage: 10
};

export function validateVenueDraft(draft) {
  const venue = {
    name: String(draft.name ?? "").trim(),
    timezone: String(draft.timezone ?? "").trim(),
    type: String(draft.type ?? "").trim(),
    primaryLanguage: String(draft.primaryLanguage ?? "").trim(),
    secondaryLanguage: String(draft.secondaryLanguage ?? "").trim() || undefined
  };
  const errors = {};

  for (const field of ["name", "timezone", "type", "primaryLanguage"]) {
    if (!venue[field]) errors[field] = "Required";
  }
  for (const [field, limit] of Object.entries(limits)) {
    if (venue[field]?.length > limit) errors[field] = `Maximum ${limit} characters`;
  }

  return { venue, errors, valid: Object.keys(errors).length === 0 };
}
