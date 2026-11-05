
const API_URL = "http://localhost:5190/api/planets";

export async function getPlanets() {
    const response = await fetch(API_URL);

    if (!response.ok) {
        throw new Error("Failed to fetch planets");
    }

    return response.json();
}
