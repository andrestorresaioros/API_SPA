import { useEffect, useMemo, useState } from "react";
import { getCategories } from "../api/categoryApi";

const INITIAL_FORM = {
  productName: "",
  categoryId: "",
  quantityPerUnit: "",
  unitPrice: "0",
  unitsInStock: "0",
  unitsOnOrder: "0",
  reorderLevel: "0",
  discontinued: false
};

function mapDataToForm(data) {
  if (!data) {
    return INITIAL_FORM;
  }

  return {
    productName: data.productName ?? "",
    categoryId: String(data.categoryId ?? ""),
    quantityPerUnit: data.quantityPerUnit ?? "",
    unitPrice: String(data.unitPrice ?? 0),
    unitsInStock: String(data.unitsInStock ?? 0),
    unitsOnOrder: String(data.unitsOnOrder ?? 0),
    reorderLevel: String(data.reorderLevel ?? 0),
    discontinued: Boolean(data.discontinued)
  };
}

function mapFormToPayload(form) {
  return {
    productName: form.productName.trim(),
    categoryId: Number(form.categoryId),
    quantityPerUnit: form.quantityPerUnit.trim() || null,
    unitPrice: Number(form.unitPrice),
    unitsInStock: Number(form.unitsInStock),
    unitsOnOrder: Number(form.unitsOnOrder),
    reorderLevel: Number(form.reorderLevel),
    discontinued: form.discontinued
  };
}

export default function ProductForm({ initialData, onSubmit, submitLabel }) {
  const [form, setForm] = useState(() => mapDataToForm(initialData));
  const [categories, setCategories] = useState([]);
  const [loadingCategories, setLoadingCategories] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    setForm(mapDataToForm(initialData));
  }, [initialData]);

  useEffect(() => {
    const loadCategories = async () => {
      setLoadingCategories(true);
      setError("");

      try {
        const response = await getCategories();
        setCategories(response ?? []);
      } catch (apiError) {
        setError(apiError.message ?? "No fue posible cargar categorias.");
      } finally {
        setLoadingCategories(false);
      }
    };

    loadCategories();
  }, []);

  const hasCategories = useMemo(() => categories.length > 0, [categories]);
  const quantityPerUnitLength = form.quantityPerUnit.length;

  const handleChange = (event) => {
    const { name, value, type, checked } = event.target;
    setForm((current) => ({
      ...current,
      [name]: type === "checkbox" ? checked : value
    }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");

    if (!hasCategories) {
      setError("Debes crear al menos una categoria antes de guardar un producto.");
      return;
    }

    if (!form.categoryId) {
      setError("Selecciona una categoria.");
      return;
    }

    if (form.quantityPerUnit.trim().length > 50) {
      setError("Cantidad por unidad permite maximo 50 caracteres.");
      return;
    }

    setSubmitting(true);

    try {
      await onSubmit(mapFormToPayload(form));
    } catch (apiError) {
      setError(apiError.message ?? "No pudimos guardar el producto. Revisa los datos e intenta nuevamente.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="card">
      <form onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="field">
            <label htmlFor="productName">Nombre</label>
            <input
              id="productName"
              name="productName"
              value={form.productName}
              onChange={handleChange}
              required
            />
          </div>

          <div className="field">
            <label htmlFor="categoryId">Categoria</label>
            <select
              id="categoryId"
              name="categoryId"
              value={form.categoryId}
              onChange={handleChange}
              required
              disabled={loadingCategories}
            >
              <option value="">Selecciona...</option>
              {categories.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.categoryName}
                </option>
              ))}
            </select>
          </div>

          <div className="field">
            <label htmlFor="quantityPerUnit">Cantidad por unidad</label>
            <input
              id="quantityPerUnit"
              name="quantityPerUnit"
              value={form.quantityPerUnit}
              onChange={handleChange}
              maxLength={50}
            />
            <small className="field-hint">{quantityPerUnitLength}/50 caracteres</small>
          </div>

          <div className="field">
            <label htmlFor="unitPrice">Precio unitario</label>
            <input
              id="unitPrice"
              name="unitPrice"
              type="number"
              min="0"
              step="0.01"
              value={form.unitPrice}
              onChange={handleChange}
              required
            />
          </div>

          <div className="field">
            <label htmlFor="unitsInStock">Stock</label>
            <input
              id="unitsInStock"
              name="unitsInStock"
              type="number"
              min="0"
              step="1"
              value={form.unitsInStock}
              onChange={handleChange}
              required
            />
          </div>

          <div className="field">
            <label htmlFor="unitsOnOrder">Unidades en orden</label>
            <input
              id="unitsOnOrder"
              name="unitsOnOrder"
              type="number"
              min="0"
              step="1"
              value={form.unitsOnOrder}
              onChange={handleChange}
              required
            />
          </div>

          <div className="field">
            <label htmlFor="reorderLevel">Nivel de reorden</label>
            <input
              id="reorderLevel"
              name="reorderLevel"
              type="number"
              min="0"
              step="1"
              value={form.reorderLevel}
              onChange={handleChange}
              required
            />
          </div>

          <div className="field">
            <label htmlFor="discontinued">Descontinuado</label>
            <input
              id="discontinued"
              name="discontinued"
              type="checkbox"
              checked={form.discontinued}
              onChange={handleChange}
            />
          </div>
        </div>

        <div className="actions-row">
          <button type="submit" disabled={submitting || loadingCategories}>
            {submitting ? "Guardando..." : submitLabel}
          </button>
        </div>
      </form>

      {error && <div className="alert error">{error}</div>}
    </div>
  );
}
