import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import TopicIcon from "../components/TopicIcon";
import { usePageTitle } from "../hooks/usePageTitle";
import { getTopics, QUESTIONS_PER_ROUND } from "../services/challengeService";
import type { Topic } from "../types/Question";

// One card per subject; clicking a card starts a practice round (like Challenge/Topics in Subapp 1)
function TopicsPage() {
    usePageTitle("Challenges");

    const [topics, setTopics] = useState<Topic[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function loadTopics() {
            try {
                setTopics(await getTopics());
            } catch {
                setError("Could not load the challenges.");
            } finally {
                setLoading(false);
            }
        }

        loadTopics();
    }, []);

    return (
        <>
            <section className="gs-page-intro">
                <h1>Challenges</h1>
                <p>
                    Pick a subject and practice {QUESTIONS_PER_ROUND} random questions at your own pace.
                    Each subject is also the mission on one planet.
                </p>
            </section>

            {/* aria-live: screen readers hear when loading ends or an error appears */}
            <div aria-live="polite">
                {loading && <p className="gs-page-intro">Loading challenges...</p>}
                {error && <p className="gs-page-intro" role="alert">{error}</p>}
                {!loading && !error && topics.length === 0 && (
                    <p className="gs-page-intro">No challenges found.</p>
                )}
            </div>

            {topics.length > 0 && (
                <section className="gs-topic-grid" aria-label="Subjects">
                    {topics.map(topic => (
                        // encodeURIComponent: "Access Control & Authentication" has a & and spaces
                        <Link
                            key={topic.subject}
                            to={`/challenges/practice/${encodeURIComponent(topic.subject)}`}
                            className="gs-topic-card"
                        >
                            <div className="gs-topic-icon" aria-hidden="true">
                                <TopicIcon iconKey={topic.iconKey} />
                            </div>
                            <h2 className="gs-topic-title">{topic.subject}</h2>
                            <div className="gs-topic-meta">
                                <span className="gs-topic-tag">{topic.difficulty}</span>
                                <span>{topic.questionCount} questions</span>
                            </div>
                        </Link>
                    ))}
                </section>
            )}
        </>
    );
}

export default TopicsPage;
