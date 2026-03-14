# Deployment Guide

## Backend Deployment (Railway)

### Prerequisites
1. Create a GitHub account if you don't have one
2. Create a Railway account at https://railway.app
3. Push your code to GitHub

### Steps:
1. **Push to GitHub:**
   ```bash
   git init
   git add .
   git commit -m "Initial commit"
   git branch -M main
   git remote add origin YOUR_GITHUB_REPO_URL
   git push -u origin main
   ```

2. **Deploy on Railway:**
   - Go to https://railway.app
   - Click "Start a New Project"
   - Select "Deploy from GitHub repo"
   - Choose your cricket-live-score repository
   - Railway will auto-detect the .NET project
   - Set the root directory to: `backend/CricketAPI`
   - Deploy!

3. **Get your API URL:**
   - After deployment, Railway will provide a URL like: `https://your-app.railway.app`

## Frontend Deployment (Vercel)

### Steps:
1. **Deploy on Vercel:**
   - Go to https://vercel.com
   - Click "New Project"
   - Import your GitHub repository
   - Set the root directory to: `frontend/cricket-app`
   - Add environment variable:
     - Name: `REACT_APP_API_URL`
     - Value: `https://your-railway-app.railway.app` (from step above)
   - Deploy!

2. **Your app will be live at:**
   - `https://your-app.vercel.app`

## Alternative: Local Network Access

If you want to test on your local network:

1. **Start backend:**
   ```bash
   cd backend/CricketAPI
   dotnet run --urls "http://0.0.0.0:5205"
   ```

2. **Update frontend .env:**
   ```
   REACT_APP_API_URL=http://YOUR_LOCAL_IP:5205
   ```

3. **Start frontend:**
   ```bash
   cd frontend/cricket-app
   npm start
   ```

Your app will be accessible from other devices on your network!