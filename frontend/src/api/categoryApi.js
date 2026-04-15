import { httpRequest } from "./httpClient";

export function getCategories() {
  return httpRequest("/category");
}
