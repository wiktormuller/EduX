export type JsonWebToken = {
    id: string
    accessToken: string;
    refreshToken: string;
    expires: number,
    role: string,
    email: string
    claims: Map<string, string[]>
};

export type LoginRequest = {
    email: string,
    password: string
};