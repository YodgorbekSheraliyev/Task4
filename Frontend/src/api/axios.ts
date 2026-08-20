import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;
    const message = error.response?.data?.message;

    if (status === 401) {
      localStorage.removeItem("token");

      sessionStorage.setItem(
        "authMessage",
        message || "Your email is not verified. Please click link sent to your email",
      );

      window.location.href = "/login";
    }

    if (status === 403) {
      sessionStorage.setItem(
        "authMessage",
        message || "Your account has been blocked. You can not perform any action",
      );

      window.location.href = "/login";
    }

    return Promise.reject(error);
  },
);

export default api;
