import { useEffect, useState } from "react";
import api from "../api/axios";
import Toolbar from "../components/Toolbar";
import { useNavigate } from "react-router-dom";

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

export default function Users() {
  const [users, setUsers] = useState<User[]>([]);
  const [selected, setSelected] = useState<number[]>([]);
  const navigate = useNavigate();

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
    setSelected((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id],
    );
  };

  const toggleAll = () => {
    if (selected.length === users.length) setSelected([]);
    else setSelected(users.map((u) => u.id));
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/");
  };

  const loadUsers = async () => {
    try {
      const res = await api.get("/users/all");
      setUsers(
        res.data.sort(
          (a: User, b: User) =>
            new Date(b.lastLogin).getTime() - new Date(a.lastLogin).getTime(),
        ),
      );
    } catch (error) {
      console.error("Failed to load users:", error);
    }
  };

  return (
    <div className="bg-light min-vh-100">
      {/* Navigation header */}
      <nav className="navbar navbar-light bg-white shadow-sm px-3">
        <span className="navbar-brand mb-0 h5">User Management</span>
        <button
          className="btn btn-outline-danger btn-sm"
          onClick={handleLogout}
        >
          Logout
        </button>
      </nav>

      {/* Main content */}
      <div className="container py-4">
        <div className="card shadow-sm">
          <div className="card-body">
            <h5 className="card-title mb-3">Users</h5>
            <Toolbar selected={selected} refresh={loadUsers} />
            <div className="table-responsive">
              <table className="table table-hover align-middle">
                <thead className="table-light">
                  <tr>
                    <th>
                      <input
                        type="checkbox"
                        checked={
                          selected.length === users.length && users.length > 0
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
