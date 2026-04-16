import { useEffect, useState } from "react";
import { createCategory, getCategories } from "../api/categoryApi";

const INITIAL_FORM = {
  categoryName: "",
  description: ""
};

export default function CategoriesPage() {
  const [form, setForm] = useState(INITIAL_FORM);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const loadCategories = async () => {
    setLoading(true);
    setError("");

    try {
      const response = await getCategories();
      setCategories(response ?? []);
    } catch {
      setError("No fue posible cargar categorias.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCategories();
  }, []);

  const handleChange = (event) => {
    const { name, value } = event.target;
    setForm((current) => ({
      ...current,
      [name]: value
    }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setSuccess("");

    if (!form.categoryName.trim()) {
      setError("El nombre de categoria es obligatorio.");
      return;
    }

    setSubmitting(true);

    try {
      await createCategory({
        categoryName: form.categoryName.trim(),
        description: form.description.trim() || null
      });

      setForm(INITIAL_FORM);
      setSuccess("Categoria creada correctamente.");
      await loadCategories();
    } catch (apiError) {
      setError(apiError.message ?? "No fue posible crear la categoria.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="card">
      <h2 style={{ marginTop: 0 }}>Categorias</h2>
      <p>Crea categorias para habilitar la creacion de productos.</p>

      <form onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="field">
            <label htmlFor="categoryName">Nombre de categoria</label>
            <input
              id="categoryName"
              name="categoryName"
              value={form.categoryName}
              onChange={handleChange}
              maxLength={100}
              required
            />
          </div>

          <div className="field">
            <label htmlFor="description">Descripcion</label>
            <input
              id="description"
              name="description"
              value={form.description}
              onChange={handleChange}
              maxLength={1000}
            />
          </div>
        </div>

        <div className="actions-row">
          <button type="submit" disabled={submitting}>
            {submitting ? "Guardando..." : "Crear categoria"}
          </button>
        </div>
      </form>

      {error && <div className="alert error">{error}</div>}
      {success && <div className="alert success">{success}</div>}

      {loading ? (
        <p>Cargando categorias...</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Nombre</th>
              <th>Descripcion</th>
            </tr>
          </thead>
          <tbody>
            {categories.length === 0 ? (
              <tr>
                <td colSpan={3}>No hay categorias registradas.</td>
              </tr>
            ) : (
              categories.map((category) => (
                <tr key={category.id}>
                  <td>{category.id}</td>
                  <td>{category.categoryName}</td>
                  <td>{category.description || "-"}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
