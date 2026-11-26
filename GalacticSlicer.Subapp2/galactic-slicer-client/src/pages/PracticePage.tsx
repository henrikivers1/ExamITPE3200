import { useState } from "react";
import { useParams } from "react-router-dom";
import PracticeQuiz from "../components/PracticeQuiz";
import { usePageTitle } from "../hooks/usePageTitle";
import { QUESTIONS_PER_ROUND } from "../services/challengeService";

// /challenges/practice/:subject - a practice round in one subject (like Challenge/Practice in Subapp 1)
function PracticePage() {
    // React Router decodes the URL, so "Access%20Control%20%26%20Authentication" comes back readable
    const { subject = "" } = useParams();
    usePageTitle(`${subject} practice`);

    // "Practice again" bumps the round; the new key gives PracticeQuiz fresh state and new questions
    const [round, setRound] = useState(0);

    return (
        <>
            <section className="gs-page-intro">
                <h1>{subject}</h1>
                <p>
                    Answer each question to see if you got it right, and why. Practice as often as you like:
                    you get {QUESTIONS_PER_ROUND} new random questions every time.
                </p>
            </section>

            <PracticeQuiz
                key={`${subject}-${round}`}
                subject={subject}
                focusOnStart={round > 0}
                onRestart={() => setRound(current => current + 1)}
            />
        </>
    );
}

export default PracticePage;
