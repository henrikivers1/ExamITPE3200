import ComingSoon from "../components/ComingSoon";

// Needs the challenge endpoints from the API (CRUD + search)
function ChallengesPage() {
    return (
        <ComingSoon
            title="Challenges"
            text="Create your own challenges and practice cybersecurity questions."
            features={[
                "Practice questions by subject, with explanations",
                "Create, edit and delete your own challenges",
                "Search your challenges by text and subject",
            ]}
        />
    );
}

export default ChallengesPage;
