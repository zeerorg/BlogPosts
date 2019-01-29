# React hooks and their dependence on each other

Pretty much everyone knows about react hooks already. From react docs:

> Hooks are an upcoming feature that lets you use state and other React features without writing a class

One very notable difference between hooks and classes is that there can be multiple state variables and you are not limited to fiddling with a single state object. This obviously provides greater separation of concerns and more composability for effects inside a component, but if done wrong it can introduce weird bugs. We'll look at some pointers that might help you with dealing that complexity.

## Fetching data depending on some state variable

This is the most common case of state dependence. You want to fetch some data, that depends on a state variable. For example: Getting different quotes based on some user input. I'll go ahead and give you the code sample (view on [CodeSandbox](https://codesandbox.io/s/92klk88mqr)):

```jsx
function App() {
  let [inp, setInp] = useState("");
  let [quote, setQuote] = useState("");

  useEffect(
    () => {
      setQuote(GetQuote(inp));
    },
    // This is important
    [inp]
  );

  return (
    <div className="App">
      <input
        type="text"
        value={inp}
        onChange={event => setInp(event.target.value)}
      />
      <h1>{quote}</h1>
    </div>
  );
}
```

You'll see that the `useEffect` hook has the second argument as an array which contains a single value, the state variable it depends on. This effectively tells useEffect to be triggered only when the state changes. This is a basic skeleton of how you can chain hooks in a sane way. You can add more states and effects to this components and chain them using this method so they don't clash with each other.

> This is a case of "Effects depending on state"

Fetching data is only a simple effect, other types of effects like manipulating DOM nodes or updating a server can be done in a similar manner. The state variable can be something like the position of the cursor or it can even be something obtained from a hook library.

## Don't call hooks in nested scope

This has been said a lot of times before, but I should just say it again. Hooks are impure and their behavior in nested scopes is undefined. Below are some tips to shift the scope.

1. For conditional effects, rely on the second useEffect argument and comparing the values inside the effect itself.
2. Similarly, for effects in a loop, shift the loop in the effect callback scope.
3. Always initialize state with a default value and don't put it in condtional scope.
4. Set the state as an array or object instead of initializing states in a loop.

## This is not the end

These are somethings I keep in mind while using hooks. You'll encounter and discover a lot more patterns from your use of hooks, when you do, please share them with the rest of the community.