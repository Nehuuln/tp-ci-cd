const express = require("express");
const request = require("supertest");

const mockCommand = jest.fn().mockResolvedValue({});

const mockToArray = jest.fn().mockResolvedValue([
  { name: "Test", email: "test@test.com" }
]);

const mockDb = {
  command: mockCommand,
  collection: () => ({
    find: () => ({
      limit: () => ({ toArray: mockToArray })
    })
  })
};

function createApp(db) {
  const app = express();
  app.use(express.json());

  app.get("/health", async (_req, res) => {
    try {
      await db.command({ ping: 1 });
      return res.status(200).json({ status: "ok", db: "ok" });
    } catch {
      return res.status(503).json({ status: "degraded", db: "down" });
    }
  });

  app.get("/users", async (_req, res) => {
    const users = await db.collection("users").find({}).limit(50).toArray();
    return res.status(200).json(users);
  });

  return app;
}

const app = createApp(mockDb);

describe("GET /health", () => {
  it("return status ok when db is up", async () => {
    const res = await request(app).get("/health");
    expect(res.status).toBe(200);
    expect(res.body).toEqual({ status: "ok", db: "ok" });
  });

  it("return status degraded when db is down", async () => {
    mockCommand.mockRejectedValueOnce(new Error("db down"));
    const res = await request(app).get("/health");
    expect(res.status).toBe(503);
    expect(res.body.status).toBe("degraded");
  });
});

describe("GET /users", () => {
  it("return the list of users", async () => {
    const res = await request(app).get("/users");
    expect(res.status).toBe(200);
    expect(Array.isArray(res.body)).toBe(true);
    expect(res.body[0].name).toBe("Test");
  });
});