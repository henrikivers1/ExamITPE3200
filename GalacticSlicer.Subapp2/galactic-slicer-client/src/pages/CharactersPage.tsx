import ComingSoon from "../components/ComingSoon";

// Needs the character endpoints from the API (buy, equip)
function CharactersPage() {
    return (
        <ComingSoon
            title="Shop"
            text="Spend your Credits on character badges."
            features={[
                "Browse character badges and their prices",
                "Buy characters with the Credits you earn in missions",
                "Equip or unequip the badge shown next to your name",
            ]}
        />
    );
}

export default CharactersPage;
