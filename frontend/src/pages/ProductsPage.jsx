import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getCategories } from "../api/categoryApi";
import { deleteProduct, getProducts } from "../api/productApi";

const PAGE_SIZE = 10;

export default function ProductsPage() {
  const [categories, setCategories] = useState([]);
  const [productsPage, setProductsPage] = useState({
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: PAGE_SIZE,
    totalPages: 1
  });
  const [searchIDInput, setSearchIDInput] = useState("");
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [searchProductId, setSearchProductId] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [actionError, setActionError] = useState("");
  const [actionSuccess, setActionSuccess] = useState("");

  useEffect(() => {
    const loadCategories = async () => {
      try {
        const response = await getCategories();
        setCategories(response ?? []);
      } catch {
        // Fail silently. Products list still works without category filter options.
      }
    };

    loadCategories();
  }, []);

  const loadProducts = useCallback(async () => {
    setLoading(true);
    setError("");

    try {
      const response = await getProducts({
        pageNumber,
        pageSize: PAGE_SIZE,
        search,
        productId: searchProductId || undefined,
        categoryId: categoryId || undefined
      });

      setProductsPage(response);
    } catch {
      setError("No pudimos cargar el listado de productos. Intenta nuevamente.");
    } finally {
      setLoading(false);
    }
  }, [categoryId, pageNumber, search, searchProductId]);

  useEffect(() => {
    loadProducts();
  }, [loadProducts]);

  const handleSearchSubmit = (event) => {
    event.preventDefault();

    const trimmedId = searchIDInput.trim();

    if (trimmedId && !/^\d+$/.test(trimmedId)) {
      setError("El ID del producto debe ser un numero entero positivo.");
      return;
    }

    setError("");
    setPageNumber(1);
    setSearch(searchInput.trim());
    setSearchProductId(trimmedId ? Number(trimmedId) : "");
  };

  const handleCategoryChange = (event) => {
    setPageNumber(1);
    setCategoryId(event.target.value);
  };

  const handleDeleteProduct = async (product) => {
    setActionError("");
    setActionSuccess("");

    const confirmed = window.confirm(`Se eliminara el producto \"${product.productName}\". Deseas continuar?`);

    if (!confirmed) {
      return;
    }

    try {
      await deleteProduct(product.id);
      setActionSuccess("Producto eliminado correctamente.");
      await loadProducts();
    } catch (apiError) {
      setActionError(apiError.message ?? "No fue posible eliminar el producto.");
    }
  };

  return (
    <div className="card">
      <div className="actions-row" style={{ justifyContent: "space-between" }}>
        <h2 style={{ margin: 0 }}>Productos</h2>
        <Link to="/products/new">
          <button type="button">Nuevo producto</button>
        </Link>
      </div>

      <form onSubmit={handleSearchSubmit} className="actions-row">
        <input
          placeholder="Buscar por nombre"
          value={searchInput}
          onChange={(event) => setSearchInput(event.target.value)}
          style={{ minWidth: 260 }}
        />

        <input
          placeholder="Buscar por ID"
          value={searchIDInput}
          onChange={(event) => setSearchIDInput(event.target.value)}
          style={{ minWidth: 260 }}
        />

        <select value={categoryId} onChange={handleCategoryChange}>
          <option value="">Todas las categorias</option>
          {categories.map((category) => (
            <option key={category.id} value={category.id}>
              {category.categoryName}
            </option>
          ))}
        </select>

        <button type="submit" className="secondary">
          Buscar
        </button>
      </form>

      {error && <div className="alert error">{error}</div>}
      {actionError && <div className="alert error">{actionError}</div>}
      {actionSuccess && <div className="alert success">{actionSuccess}</div>}

      {loading ? (
        <p>Cargando...</p>
      ) : (
        <>
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Nombre</th>
                <th>Categoria</th>
                <th>Precio</th>
                <th>Stock</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {productsPage.items.length === 0 ? (
                <tr>
                  <td colSpan={6}>No hay productos para mostrar.</td>
                </tr>
              ) : (
                productsPage.items.map((product) => (
                  <tr key={product.id}>
                    <td>{product.id}</td>
                    <td>{product.productName}</td>
                    <td>{product.categoryName}</td>
                    <td>{Number(product.unitPrice).toFixed(2)}</td>
                    <td>{product.unitsInStock}</td>
                    <td>
                      <div className="table-actions">
                        <Link to={`/products/${product.id}/edit`}>
                          <button type="button" className="secondary">
                            Editar
                          </button>
                        </Link>
                        <button
                          type="button"
                          className="secondary"
                          onClick={() => handleDeleteProduct(product)}
                        >
                          Eliminar
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>

          <div className="pagination">
            <button
              type="button"
              className="secondary"
              onClick={() => setPageNumber((value) => Math.max(1, value - 1))}
              disabled={productsPage.pageNumber <= 1}
            >
              Anterior
            </button>

            <span>
              Pagina {productsPage.pageNumber} de {productsPage.totalPages || 1} (Total: {productsPage.totalCount})
            </span>

            <button
              type="button"
              className="secondary"
              onClick={() => setPageNumber((value) => value + 1)}
              disabled={productsPage.pageNumber >= (productsPage.totalPages || 1)}
            >
              Siguiente
            </button>
          </div>
        </>
      )}
    </div>
  );
}
