const express = require("express");
const { MongoClient } = require("mongodb");

const app = express();
app.use(express.json());

const port = Number(process.env.PORT || 3000);
const dbHost = process.env.DB_HOST || "localhost";
const dbPort = process.env.DB_PORT || "27017";
const dbName = process.env.DB_NAME || "appdb";
const dbUser = process.env.DB_USER || "appuser";
const dbPassword = process.env.DB_PASSWORD || "mypassword";

const mongoUri = `mongodb://${encodeURIComponent(dbUser)}:${encodeURIComponent(dbPassword)}@${dbHost}:${dbPort}/${dbName}?authSource=admin`;

let db;

app.get("/health", async (_req, res) => {
  try {
    await db.command({ ping: 1 });
    return res.status(200).json({ status: "ok", db: "ok" });
  } catch (error) {
    return res.status(503).json({ status: "degraded", db: "down" });
  }
});

app.post("/users", async (req, res) => {
  const { name, email } = req.body || {};
  if (!name || !email) {
    return res.status(400).json({ error: "name and email are required" });
  }

  const result = await db.collection("users").insertOne({
    name,
    email,
    createdAt: new Date()
  });

  return res.status(201).json({ id: result.insertedId.toString() });
});

app.get("/users", async (_req, res) => {
  const users = await db.collection("users").find({}).limit(50).toArray();
  return res.status(200).json(users);
});

async function start() {
  const client = new MongoClient(mongoUri);
  await client.connect();
  db = client.db(dbName);

  app.listen(port, () => {
    console.log(`API listening on port ${port}`);
  });
}

start().catch((error) => {
  console.error("Failed to start API:", error);
  process.exit(1);
});
