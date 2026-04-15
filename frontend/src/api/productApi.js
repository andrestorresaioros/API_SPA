import { httpRequest } from "./httpClient";

export function getProducts(params) {
  return httpRequest("/products", {
    query: params
  });
}

export function getProductById(id) {
  return httpRequest(`/products/${id}`);
}

export function createProduct(payload) {
  return httpRequest("/product", {
    method: "POST",
    body: payload,
    auth: true
  });
}

export function updateProduct(id, payload) {
  return httpRequest(`/products/${id}`, {
    method: "PUT",
    body: payload,
    auth: true
  });
}

export function deleteProduct(id) {
  return httpRequest(`/products/${id}`, {
    method: "DELETE",
    auth: true
  });
}
