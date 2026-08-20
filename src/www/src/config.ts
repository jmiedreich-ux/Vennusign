export const apiBaseUrl = (
  import.meta.env.VITE_API_URL ?? ""
).replace(/\/$/, "");

export const backOfficeBaseUrl = (
  import.meta.env.VITE_BACK_OFFICE_URL ?? "https://dev.back-office.vennusign.com"
).replace(/\/$/, "");

export const signupUrl = `${backOfficeBaseUrl}/signup`;
export const signinUrl = `${backOfficeBaseUrl}/signin`;
