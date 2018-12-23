# Caching Data in frontend

_We'll talk about ways you can effectively cache requests sent to backend without fidelling with backend HTTP headers._

Coordinating data between state changes can be hard, consider an application where you are sending request to the backend to get a list of posts to be displayed, and when user clicks on a post the application sends another request to get that post data. Now the backend is fairly simple so you get the precise data for that post, but you also want to display the title of next post and previous post, and maybe some sponsor data which is shared during the user's session. Requesting data again from backend is wasteful in such cases so the frontend has some options at this point.

## Managing Javascript state

The frontend can keep a track of the data that will be reused. Storing that data in a global variable, passing state to higher levels, or using an api like [React Context](https://reactjs.org/docs/context.html). There are problems with each of these approaches, global variables are evil by default. Passing data around components or maintaining it in a context like api can become messy as number of requests grow.

## Using a state management framework

This is a very typical use case for a JavaScript state management framework like [redux](https://redux.js.org/). They provide a way to manage complex application data. But if you are anything like me, the idea of introducing a new framework and overhead of learning to code around it can be a daunting task. These frameworks can also force opinionated design on your frontend so for someone who is not familiar with one, it can be a big commitment.

## Browser Storage (the real MVP)

We come at our final answer, browser storage api. It is a key value store which is managed by the browser. There are two types of browser storages: local and session. Both of these provide a similar api, but, while the local storage is never cleared, session storage is cleared after the tab is closed. This approach is a lot better than our previous approaches as it's **not as barebones as passing the state around** and **not as complex as learning a new state management framework**.

Browser storage api includes only two operations, `setItem` and `getItem` and as you can probably guess `setItem` stores the value for a given key and `getItem` retrieves the value. We are free from managing the state ourselves and can just provide the key and value for the data to store and retrieve it later.

An example use of this api is demonstrated by creating a function that invokes a GET request to a url and returns the result as a promise.

```javascript
// Without caching
function FetchData(url) {
  return fetch(url).then(res => res.text())
}

// With Caching
function FetchData(url) {
  let storageData = sessionStorage.getItem(url);
  if (storageData === null) {
    return fetch(url).then(res => res.text()).then(textData => {
      sessionStorage.setItem(url, textData)
      return textData
    })
  }
  return Promise.resolve(storageData);
}
```

We treat the provided url as our key and store the fetched data, so that any subsequent request is fulfilled from the cache. The best part about this approach is that it is easier to understand and doesn't intrude with our frontend code. It is also the best solution for our problem in this case.

![Basic Architecture](https://zeerorgprocessedblog.blob.core.windows.net/photos/frontend-caching.png "Browser storage api")

Caching is one of the most effective techniques to optimize performance and user experience. Storing request data on the frontend provides speedy navigation and greater control over what is stored by avoiding unnecessary requests to the backend.
