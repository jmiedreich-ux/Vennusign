export const apiBaseUrl = (
  import.meta.env.VITE_VENNUSIGN_API_BASE_URL ?? ""
).replace(/\/$/, "");

export const backOfficeBaseUrl = (
  import.meta.env.VITE_VENNUSIGN_BACK_OFFICE_BASE_URL ?? "https://dev.back-office.vennusign.com"
).replace(/\/$/, "");

export const signupUrl = `${backOfficeBaseUrl}/signup`;
export const signinUrl = `${backOfficeBaseUrl}/signin`;
