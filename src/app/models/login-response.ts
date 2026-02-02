export interface LoginResponse {
  token?: string;
  Token?: string; // handle different casings
  expires?: string;
  roles?: string[];
}