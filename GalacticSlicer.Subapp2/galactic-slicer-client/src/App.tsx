import { Route, Routes } from "react-router-dom";
import Layout from "./components/Layout";
import HomePage from "./pages/HomePage";
import PlanetsPage from "./pages/PlanetsPage";
import ManageChallengesPage from "./pages/ManageChallengesPage";
import TopicsPage from "./pages/TopicsPage";
import PracticePage from "./pages/PracticePage";
import ShopPage from "./pages/ShopPage";
import ProgressPage from "./pages/ProgressPage";
import NotFoundPage from "./pages/NotFoundPage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";

// Every page is rendered inside Layout (navbar, main content and footer)
function App() {
    return (
        <Routes>
            <Route element={<Layout />}>
                <Route index element={<HomePage />} />
                <Route path="planets" element={<PlanetsPage />} />
                <Route path="challenges" element={<ManageChallengesPage />} />
                <Route path="challenges/topics" element={<TopicsPage />} />
                <Route path="challenges/practice/:subject" element={<PracticePage />} />
                <Route path="characters" element={<ShopPage />} />
                <Route path="progress" element={<ProgressPage />} />
                <Route path="login" element={<LoginPage />} />
                <Route path="register" element={<RegisterPage />} />
                <Route path="*" element={<NotFoundPage />} />
            </Route>
        </Routes>
    );
}

export default App;
