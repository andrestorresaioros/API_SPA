import { httpRequest } from "./httpClient";

export function getCategories() {
  return httpRequest("/category");
}

export function createCategory(payload) {
  return httpRequest("/category", {
    method: "POST",
    body: payload,
    auth: true
  });
}
