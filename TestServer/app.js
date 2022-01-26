const express = require('express')
const app = express()
const cors = require('cors')
const port = 3000

app.use(express.json()) // for parsing application/json
app.use(express.urlencoded({ extended: true })) // for parsing application/x-www-form-urlencoded
app.use(cors())

app.post('/echo', (req, res) => {
  console.log(req.body)
  res.json(req.body)
})


app.post('/api/auth/login', (req, res) => {
  res.json({
  "token": {
    "bearer": "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjYxZGQ2NTYwYTE3YjNlOTIzNzRhMTEwYiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJUZXN0IFVzZXIiLCJleHAiOjE2NDE4OTk2OTYsImlzcyI6Ikp3dEF1dGhTZXJ2ZXIiLCJhdWQiOiJKd3RBdXRoQ2xpZW50In0.JpbEnhxm-s_72P3jgFNqs4nsBlqcO9jU-zyIhtrnnMU",
    "expired": "2022-01-25T14:14:56.2093826+03:00"
  },
  "user": {
    "id": "61dd6560a17b3e92374a110b",
    "username": "Test User",
    "email": "testuser@email.com"
  }
  });
})

app.post('/api/auth/logout', (req, res) => {
  res.status(204).send('');
})

app.post('/api/auth/refresh-token', (req, res) => {
  res.json({
  "token": {
    "bearer": "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjYxZGQ2NTYwYTE3YjNlOTIzNzRhMTEwYiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJUZXN0IFVzZXIiLCJleHAiOjE2NDE4OTk2OTYsImlzcyI6Ikp3dEF1dGhTZXJ2ZXIiLCJhdWQiOiJKd3RBdXRoQ2xpZW50In0.JpbEnhxm-s_72P3jgFNqs4nsBlqcO9jU-zyIhtrnnMU",
    "expired": "2022-01-25T14:14:56.2093826+03:00"
  },
  "user": {
    "id": "61dd6560a17b3e92374a110b",
    "username": "Test User",
    "email": "testuser@email.com"
  }
  });
})

app.post('/api/auth/register', (req, res) => {
	res.status(201).json({
		id: "61dd6560a17b3e92374a110c",
		username: "Test Register User",
		email: "testregisteruser@email.com"
	});
})

app.get('/api/auth/me', (req, res) => {
	res.json({
		id: "61dd6560a17b3e92374a110b",
		username: "Test User",
		email: "testuser@email.com"
	});
})


app.get('/api/test', (req, res) => {
	res.status(401).send('');
})


app.get('/api/projects', (req, res) => {
	res.json({
		items: [
			{id: "61dd6560a17b3e92374a1101", name: "Test project 1", canEdit: true, tasksCount: 3 },
			{id: "61dd6560a17b3e92374a1102", name: "Test project 2", tasksCount: 5 },
		]
	});
})

app.get('/api/projects/*', (req, res) => {
	res.json({
		item: {id: "61dd6560a17b3e92374a1101", name: "Test project 1", users: [ 
			{ id: "61dd6560a17b3e92374a110c", username: "Test User Bob", email: "testuserbob@email.com" },
			{ id: "61dd6560a17b3e92374a110d", username: "Test User Bill", email: "testuserbill@email.com" },
			{ id: "61dd6560a17b3e92374a110e", username: "Test User Jeb", email: "testuserjeb@email.com" },
		] }
	});
})

app.post('/api/projects', (req, res) => {
	res.status(201).json({
		item: {id: "61dd6560a17b3e92374a1103", name: "Test project 3"}
	});
})

app.put('/api/projects/*/name', (req, res) => {
	res.status(204).send('');
})

app.post('/api/projects/*/users/*', (req, res) => {
	res.status(204).send('');
})

app.delete('/api/projects/*/users/*', (req, res) => {
	res.status(204).send();
})

app.delete('/api/projects/*', (req, res) => {
	res.status(204).send();
})


app.get('/api/tasks/project/*', (req, res) => {
	res.json({
		items: [
			{id: "61dd6560a17b3e92374a1104", assessment: "3h", priority: "low", deadline: "2022-02-26T12:00:00.000Z", description: "Test projects description", name: "Test task 1", status: "w", canEdit: true},
			{id: "61dd6560a17b3e92374a1105", assessment: "3h 30m", priority: "medium", deadline: "2022-02-16T18:00:00.000Z", description: "Test projects description", name: "Test task 2", status: "d", canEdit: true},
			{id: "61dd6560a17b3e92374a1106", assessment: "45m", priority: "high", deadline: "2022-03-06T18:00:00.000Z", description: "Test projects description", name: "Test task 3", status: "d"},
		]
	});
})

app.get('/api/tasks/*', (req, res) => {
	res.json({
		item: {id: "61dd6560a17b3e92374a1104", projectId: "61dd6560a17b3e92374a1101", assessment: "3h", priority: "low", deadline: "2022-02-26T12:00:00.000Z", description: "Test projects description", name: "Test task 1", status: "w", canEdit: true}
	});
})

app.post('/api/tasks', (req, res) => {
	res.status(201).json({
		item: {id: "61dd6560a17b3e92374a1107", projectId: "61dd6560a17b3e92374a1101", assessment: "5h", priority: "high", deadline: "2022-04-10T12:00:00.000Z", description: "Test projects description", name: "Test task 4", status: "w", canEdit: true}
	});
})

app.put('/api/tasks/*', (req, res) => {
	res.status(204).send('');
})

app.put('/api/tasks/*/status/*', (req, res) => {
	res.status(204).send('');
})

app.delete('/api/tasks/*', (req, res) => {
	res.status(204).send();
})


app.get('/api/users/emailExists/*', (req, res) => {
	res.json({ result: true });
})

app.put('/api/users/email/*', (req, res) => {
	res.status(204).send('');
})

app.put('/api/users/name/*', (req, res) => {
	res.status(204).send('');
})

app.put('/api/users/password/*', (req, res) => {
	res.status(204).send('');
})

app.post('/api/users/restorePassword', (req, res) => {
	res.status(204).send('');
})

app.listen(port, () => {
  console.log(`Example app listening at http://localhost:${port}`)
})