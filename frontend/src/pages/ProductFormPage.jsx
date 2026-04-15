import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import ProductForm from "../components/ProductForm";
import { createProduct, getProductById, updateProduct } from "../api/productApi";

export default function ProductFormPage({ mode }) {
  const navigate = useNavigate();
  const { id } = useParams();

  const [initialData, setInitialData] = useState(null);
  const [loading, setLoading] = useState(mode === "edit");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  useEffect(() => {
    if (mode !== "edit" || !id) {
      setLoading(false);
      return;
    }

    const loadProduct = async () => {
      setLoading(true);
      setError("");

      try {
        const response = await getProductById(id);
        setInitialData(response);
      } catch (apiError) {
        setError(apiError.message ?? "No fue posible cargar el producto.");
      } finally {
        setLoading(false);
      }
    };

    loadProduct();
  }, [id, mode]);

  const handleSubmit = async (payload) => {
    if (mode === "create") {
      await createProduct(payload);
      setSuccess("Producto creado correctamente.");
      setTimeout(() => navigate("/products", { replace: true }), 450);
      return;
    }

    await updateProduct(id, payload);
    setSuccess("Producto actualizado correctamente.");
    setTimeout(() => navigate("/products", { replace: true }), 450);
  };

  return (
    <div>
      <div className="actions-row" style={{ justifyContent: "space-between" }}>
        <h2 style={{ margin: 0 }}>{mode === "create" ? "Crear producto" : "Editar producto"}</h2>
        <Link to="/products">
          <button type="button" className="secondary">
            Volver al listado
          </button>
        </Link>
      </div>

      {error && <div className="alert error">{error}</div>}
      {success && <div className="alert success">{success}</div>}

      {loading ? (
        <div className="card">Cargando producto...</div>
      ) : (
        <ProductForm
          initialData={initialData}
          onSubmit={handleSubmit}
          submitLabel={mode === "create" ? "Crear" : "Guardar cambios"}
        />
      )}
    </div>
  );
}
