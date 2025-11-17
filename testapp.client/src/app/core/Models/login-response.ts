export interface LoginResponse {
    userId:number;
    username:string;
    role:string[];
    permissions:string[];
    token:string;
}
