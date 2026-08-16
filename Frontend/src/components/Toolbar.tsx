import api from "../api/axios";

export default function Toolbar({
  selected,
  refresh,
}: {
  selected: number[];
  refresh: () => void;
}) {
  const handleAction = async (action: string) => {
    for (const id of selected) {
      if (action === "block") {
        await api.post(`/users/block`, { userId: id });
      }
      if (action === "unblock") {
        await api.post(`/users/unblock`, { userId: id });
      }
      if (action === "delete") {
        await api.delete(`/users/delete/${id}`);
      }
    }
    refresh();
  };

  return (
    <div className="btn-toolbar mb-3">
      <button
        className="btn btn-warning me-2"
        disabled={!selected.length}
        onClick={() => handleAction("block")}
      >
        Block
      </button>
      <button
        className="btn btn-secondary me-2"
        disabled={!selected.length}
        onClick={() => handleAction("unblock")}
      >
        Unblock
      </button>
      <button
        className="btn btn-danger"
        disabled={!selected.length}
        onClick={() => handleAction("delete")}
      >
        Delete
      </button>
    </div>
  );
}
