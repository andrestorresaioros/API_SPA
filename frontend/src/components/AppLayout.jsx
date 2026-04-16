import { Link, NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/useAuth";

export default function AppLayout() {
  const { session, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="app-shell">
      <header className="app-header">
        <Link className="brand" to="/products">
          API_SPA
        </Link>
        <nav className="top-nav">
          <NavLink
            to="/products"
            className={({ isActive }) => `top-nav-link${isActive ? " active" : ""}`}
          >
            Productos
          </NavLink>
          <NavLink
            to="/categories"
            className={({ isActive }) => `top-nav-link${isActive ? " active" : ""}`}
          >
            Categorias
          </NavLink>
        </nav>
        <div className="header-actions">
          <span>{session?.username}</span>
          <button type="button" className="secondary" onClick={handleLogout}>
            Logout
          </button>
        </div>
      </header>
      <main className="page-container">
        <Outlet />
      </main>
    </div>
  );
}
