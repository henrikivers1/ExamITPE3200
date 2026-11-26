import { useEffect, useState, type FormEvent } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { usePageTitle } from "../hooks/usePageTitle";
import { getTopics } from "../services/challengeService";

// Manage Challenges, like Challenge/Index in Subapp 1: search the player's own challenges,
// create a new one, or go to practice. There is no login and no challenges endpoint yet,
// so this is the visitor version (the list of own challenges is never shown).
function ManageChallengesPage() {
    usePageTitle("Manage Challenges");

    // the filters live in the address (?searchString=...&subject=...), like the GET form in Subapp 1,
    // so they stay after a reload and work with the Back button
    const [searchParams, setSearchParams] = useSearchParams();
    const searchString = searchParams.get("searchString") ?? "";
    const subject = searchParams.get("subject") ?? "";
    const isSearching = searchString !== "" || subject !== "";

    const [subjects, setSubjects] = useState<string[]>([]);

    // subjects for the dropdown
    // TODO: comes from challengeService, which has no data until the API has a challenges endpoint;
    // until then the dropdown only has "All subjects"
    useEffect(() => {
        async function loadSubjects() {
            try {
                const topics = await getTopics();
                setSubjects(topics.map(topic => topic.subject));
            } catch {
                setSubjects([]);
            }
        }

        loadSubjects();
    }, []);

    function handleSearch(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        const form = new FormData(event.currentTarget);
        const newSearch = String(form.get("searchString") ?? "").trim();
        const newSubject = String(form.get("subject") ?? "");

        // only put filled-in fields in the address
        const params: Record<string, string> = {};
        if (newSearch) {
            params.searchString = newSearch;
        }
        if (newSubject) {
            params.subject = newSubject;
        }
        setSearchParams(params);
    }

    return (
        <>
            <section className="gs-page-intro">
                <h1>Manage Challenges</h1>
                <p>Create your own challenges and find them here, or practice the subjects from the planet missions.</p>
            </section>

            {/* role="search" tells screen readers this form is the page's search.
                key: the form is rebuilt when the address changes (Search, Clear, Back button),
                so the fields always show the filters in the address */}
            <form
                key={`${searchParams.toString()}|${subjects.length}`}
                className="gs-search"
                role="search"
                onSubmit={handleSearch}
            >
                <label className="gs-sr-only" htmlFor="searchString">Search question</label>
                <input
                    id="searchString"
                    name="searchString"
                    className="gs-input gs-search-text"
                    type="search"
                    placeholder="Search question"
                    defaultValue={searchString}
                />

                <label className="gs-sr-only" htmlFor="subject">Subject</label>
                <select
                    id="subject"
                    name="subject"
                    className="gs-input gs-search-subject"
                    defaultValue={subject}
                >
                    <option value="">All subjects</option>
                    {subjects.map(name => (
                        <option key={name} value={name}>{name}</option>
                    ))}
                </select>

                <button type="submit" className="gs-btn gs-btn--primary">Search</button>
                <button type="button" className="gs-btn gs-btn--outline" onClick={() => setSearchParams({})}>Clear</button>
            </form>

            {/* there are no own challenges to search yet, so any search has no results */}
            {isSearching && (
                <p className="challenge-empty" role="status">No challenges match your search.</p>
            )}

            <div className="gs-button-row gs-button-row--large">
                {/* TODO: go to the Create form once login and the challenge CRUD endpoints exist.
                    Creating needs an account, so for now it goes to Log in (like Subapp 1 for visitors). */}
                <Link to="/login" className="gs-btn gs-btn--primary">Create New Challenge</Link>
                <Link to="/challenges/topics" className="gs-btn gs-btn--practice">Practice Challenges</Link>
            </div>

            {/* the list only has the player's own challenges, so explain why it is empty */}
            {/* TODO: show the player's own challenges (cards with Details / Edit / Delete) once
                login and GET /api/challenges exist */}
            <p className="challenge-empty">
                <Link to="/login">Log in</Link> to see and manage the challenges you have made.
            </p>
        </>
    );
}

export default ManageChallengesPage;
