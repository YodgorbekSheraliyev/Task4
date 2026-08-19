import { useEffect, useState } from "react";
import api from "../api/axios";
import Toolbar from "../components/Toolbar";
import { useNavigate } from "react-router-dom";
import { jwtDecode } from "jwt-decode";

const ID_KEY = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

interface User {
  id: number;
  name: string;
  email: string;
  status: UserStatus;
  lastLogin: string;
}

enum UserStatus {
  Active = 0,
  Blocked = 1,
}

const statusLabels: Record<UserStatus, string> = {
  [UserStatus.Active]: "Active",
  [UserStatus.Blocked]: "Blocked",
};

function getCurrentUserId(): number | null {
  const token = localStorage.getItem("token");
  if (!token) return null;
  try {
    const payload = jwtDecode<any>(token);

    return payload[ID_KEY] ? Number(payload[ID_KEY]) : null;
  } catch {
    return null;
  }
}

export default function Users() {
  const [users, setUsers] = useState<User[]>([]);
  const [selected, setSelected] = useState<number[]>([]);
  const navigate = useNavigate();
  const currentUserId = getCurrentUserId();

  const selectableUsers = users.filter((u) => u.id !== currentUserId);

  useEffect(() => {
    api.get("/users/all").then((res) => {
      const sorted = res.data.sort(
        (a: User, b: User) =>
          new Date(b.lastLogin).getTime() - new Date(a.lastLogin).getTime(),
      );
      setUsers(sorted);
    });
  }, []);

  const toggleSelect = (id: number) => {
    if (id === currentUserId) return;

    setSelected((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id],
    );
  };

  const toggleAll = () => {
    const selectableIds = selectableUsers.map((u) => u.id);

    if (selected.length === selectableIds.length && selectableIds.length > 0) {
      setSelected([]);
      return;
    }

    setSelected(selectableIds);
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/");
  };

  const loadUsers = async () => {
    try {
      const res = await api.get("/users/all");
      const sorted = [...res.data].sort(
        (a: User, b: User) =>
          new Date(b.lastLogin).getTime() - new Date(a.lastLogin).getTime(),
      );

      setUsers(sorted);
      setSelected((prev) =>
        prev.filter(
          (id) => id !== currentUserId && sorted.some((user) => user.id === id),
        ),
      );
    } catch (error) {
      console.error("Failed to load users:", error);
    }
  };

  return (
    <div className="bg-light min-vh-100">
      <nav className="navbar navbar-light bg-white shadow-sm px-3">
        <span className="navbar-brand mb-0 h5">User Management</span>
        <button
          className="btn btn-outline-danger btn-sm"
          onClick={handleLogout}
        >
          Logout
        </button>
      </nav>

      <div className="container py-4">
        <div className="card shadow-sm">
          <div className="card-body">
            <h5 className="card-title mb-3">Users</h5>
            <Toolbar
              selected={selected.filter((id) => id !== currentUserId)}
              refresh={loadUsers}
            />
            <div className="table-responsive">
              <table className="table table-hover align-middle">
                <thead className="table-light">
                  <tr>
                    <th>
                      <input
                        type="checkbox"
                        checked={
                          selectableUsers.length > 0 &&
                          selected.length === selectableUsers.length
                        }
                        onChange={toggleAll}
                      />
                    </th>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Status</th>
                    <th>Last Login</th>
                  </tr>
                </thead>
                <tbody>
                  {users.map((u) => (
                    <tr key={u.id}>
                      <td>
                        <input
                          type="checkbox"
                          checked={selected.includes(u.id)}
                          disabled={u.id === currentUserId}
                          title={
                            u.id === currentUserId
                              ? "You cannot modify your own account"
                              : undefined
                          }
                          onChange={() => toggleSelect(u.id)}
                        />
                      </td>
                      <td>{u.name}</td>
                      <td>{u.email}</td>
                      <td>
                        <span
                          className={`badge ${
                            u.status === UserStatus.Active
                              ? "bg-success"
                              : "bg-danger"
                          }`}
                        >
                          {statusLabels[u.status]}
                        </span>
                      </td>
                      <td>{new Date(u.lastLogin).toLocaleString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
