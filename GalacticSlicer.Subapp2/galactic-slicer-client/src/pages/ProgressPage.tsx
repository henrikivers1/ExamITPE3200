import ComingSoon from "../components/ComingSoon";

// Needs login and a player progress endpoint from the API
function ProgressPage() {
    return (
        <ComingSoon
            title="Progress"
            text="Track your Credits, XP, rank and unlocked planets."
            features={[
                "See your Credits, XP and rank",
                "Follow which planets you have unlocked and completed",
                "Pick up your next mission where you left off",
            ]}
        />
    );
}

export default ProgressPage;
