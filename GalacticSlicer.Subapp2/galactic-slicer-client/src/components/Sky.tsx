interface SkyProps {
    showPlanets: boolean;
}

const skyPlanets = ["tatooine", "naboo", "coruscant", "kamino", "hoth", "endor"];

// Background sky: twinkling stars and nebulas on every page,
// plus small drifting planets everywhere except the Galaxy page (same as Subapp 1)
function Sky({ showPlanets }: SkyProps) {
    return (
        <div className="gs-sky" aria-hidden="true">
            <span className="gs-sky-stars gs-sky-stars--small"></span>
            <span className="gs-sky-stars gs-sky-stars--large"></span>
            {showPlanets && skyPlanets.map(planet => (
                <span key={planet} className={`gs-planet gs-planet--${planet} gs-sky-planet`}></span>
            ))}
        </div>
    );
}

export default Sky;
