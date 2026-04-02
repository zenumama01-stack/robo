const ___url: string = 'https://api.x.ai/v1'
 * xAI implementation is just a sub-class of the OpenAILLM that overrides the base url
@RegisterClass(BaseLLM, 'xAILLM')
export class xAILLM extends OpenAILLM {
