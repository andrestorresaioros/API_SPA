import { createContext, useCallback, useMemo, useState } from "react";
import { loginRequest } from "../api/authApi";
import { clearAuth, loadAuth, saveAuth } from "./authStorage";

export const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [session, setSession] = useState(() => loadAuth());

  const login = useCallback(async (username, password) => {
    const payload = await loginRequest({ username, password });

    const normalized = {
      accessToken: payload.accessToken,
      expiresAtUtc: payload.expiresAtUtc,
      username: payload.username,
      role: payload.role
    };

    saveAuth(normalized);
    setSession(normalized);
    return normalized;
  }, []);

  const logout = useCallback(() => {
    clearAuth();
    setSession(null);
  }, []);

  const value = useMemo(
    () => ({
      session,
      isAuthenticated: Boolean(session?.accessToken),
      login,
      logout
    }),
    [login, logout, session]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
