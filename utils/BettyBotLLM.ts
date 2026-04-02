import { BaseLLM, ChatParams, ChatResult, ChatMessageRole, ClassifyParams, ClassifyResult, SummarizeParams, SummarizeResult, ModelUsage, ErrorAnalyzer } from '@memberjunction/ai';
import axios, { AxiosError, AxiosRequestConfig } from 'axios';
import * as Config from '../config';
import { BettyResponse, SettingsResponse } from '../generic/BettyBot.types';
@RegisterClass(BaseLLM, "BettyBotLLM")
export class BettyBotLLM extends BaseLLM {
    private APIKey: string;
    private JWTToken: string;
    private TokenExpiration: Date;
        this.APIKey = apiKey;
        this.JWTToken = '';
        this.TokenExpiration = new Date();
     * Betty Bot doesn't support streaming
     * Implementation of non-streaming chat completion for Betty Bot
        try{
            //ensure the jwt token is up to date
            const jwtResponse = await this.GetJWTToken();
            if(!jwtResponse || jwtResponse.status !== 'SUCCESS'){
                const errorResult = new ChatResult(false, startTime, startTime);
                errorResult.errorMessage = jwtResponse?.errorMessage || 'Error getting JWT token';
            const endpoint: string = Config.BETTY_BOT_BASE_URL + 'response';
            const config: AxiosRequestConfig = {
                    Authorization: `Bearer ${this.JWTToken}`
            const userMessage = params.messages.find(m => m.role === ChatMessageRole.user);
            if(!userMessage){
                errorResult.errorMessage = 'No user message found in params';
            const data = {
                input: userMessage.content,
            const bettyResponse = await axios.post<BettyResponse>(endpoint, data, config);
            if(!bettyResponse || !bettyResponse.data){
                errorResult.errorMessage = 'Error getting response from Betty';
            const response = new ChatResult(true, startTime, endTime);
            // Set properties
            response.statusText = "OK";
            response.data = {
                            content: bettyResponse.data.response
                        finish_reason: "",
            response.errorMessage = "";
            response.exception = null;
             * If Betty gave us any references, add them to the response
             * as additional choices:
             * - choice[1]: Formatted text version (for display/backwards compatibility)
             * - choice[2]: Raw JSON structure (for programmatic access)
            if(bettyResponse.data.references && bettyResponse.data.references.length > 0){
                const references = bettyResponse.data.references;
                // Choice 1: Formatted text version
                let text: string = "Here are some additional resources that may help you: \n";
                for(const reference of references){
                    text += `${reference.title}: ${reference.link} \n`;
                response.data.choices.push({
                        content: text
                    index: 1
                // Choice 2: Raw structured JSON
                        content: JSON.stringify(references)
                    finish_reason: "references_json",
                    index: 2
        catch(ex){
            if(axios.isAxiosError(ex)){
                const axiosError: AxiosError = ex;
                if(axiosError.response){
                    console.log(`Error calling api: ${axiosError.response.status} - ${axiosError.response.statusText}`);
                else{
                    console.log(`Error calling api: ${axiosError.message}`);
                console.log(`Error calling api`);
            // Create a proper error result
            const errorResult = new ChatResult(false, now, now);
            errorResult.errorInfo = ErrorAnalyzer.analyzeError(ex, 'BettyBot');
     * Since BettyBot doesn't support streaming, we don't implement these methods.
     * They should never be called because SupportsStreaming returns false.
        throw new Error("BettyBot does not support streaming");
    public async GetJWTToken(forceRefresh?: boolean): Promise<SettingsResponse | null> {
            if(this.JWTToken && !forceRefresh){
                //only return the cached token if its younger than 30 minutes
                if(this.TokenExpiration.getTime() - now.getTime() < 30 * 60 * 1000){
                        token: this.JWTToken
                token: this.APIKey
            const endpoint: string = Config.BETTY_BOT_BASE_URL + 'settings';
            const response = await axios.post<SettingsResponse>(endpoint, data);
            if(response.data){
                this.JWTToken = response.data.token;
            return response.data;
            if(axios.isAxiosError(error)){
                const axiosError = error as AxiosError;
