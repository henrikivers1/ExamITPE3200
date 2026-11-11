import { Route, Routes } from "react-router-dom";
import Layout from "./components/Layout";
import HomePage from "./pages/HomePage";
import PlanetsPage from "./pages/PlanetsPage";
import ChallengesPage from "./pages/ChallengesPage";
import CharactersPage from "./pages/CharactersPage";
import ProgressPage from "./pages/ProgressPage";
import NotFoundPage from "./pages/NotFoundPage";

// Every page is rendered inside Layout (navbar, main content and footer)
function App() {
    return (
        <Routes>
            <Route element={<Layout />}>
                <Route index element={<HomePage />} />
                <Route path="planets" element={<PlanetsPage />} />
                <Route path="challenges" element={<ChallengesPage />} />
                <Route path="characters" element={<CharactersPage />} />
                <Route path="progress" element={<ProgressPage />} />
                <Route path="*" element={<NotFoundPage />} />
            </Route>
        </Routes>
    );
}

export default App;
