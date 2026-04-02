from typing import List
import rich
from pydantic import BaseModel
class Step(BaseModel):
    explanation: str
    output: str
class MathResponse(BaseModel):
    steps: List[Step]
    final_answer: str
completion = client.chat.completions.parse(
    model="gpt-4o-2024-08-06",
        {"role": "system", "content": "You are a helpful math tutor."},
        {"role": "user", "content": "solve 8x + 31 = 2"},
    response_format=MathResponse,
message = completion.choices[0].message
if message.parsed:
    rich.print(message.parsed.steps)
    print("answer: ", message.parsed.final_answer)
    print(message.refusal)
