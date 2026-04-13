import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Calendar, User, Eye, Search, ArrowRight } from 'lucide-react'
import { Card, CardContent } from '../components/ui/Card'
import Button from '../components/ui/Button'
import Input from '../components/ui/Input'
import { apiGet } from '../services/api'

interface Blog {
  id: string
  title: string
  excerpt: string
  authorName: string
  publishedAt: string
  viewCount: number
}

const Blogs = () => {
  const navigate = useNavigate()
  const [blogs, setBlogs] = useState<Blog[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searchQuery, setSearchQuery] = useState('')

  useEffect(() => {
    fetchBlogs()
  }, [])

  const fetchBlogs = async () => {
    setIsLoading(true)
    setError(null)
    try {
      const response = await apiGet<{ success: boolean; data?: Blog[] }>('/shared/blogs')
      if (response.success && response.data) {
        setBlogs(response.data)
      } else {
        setError('Failed to fetch blogs')
      }
    } catch (err: any) {
      setError(err.message || 'Failed to fetch blogs')
    } finally {
      setIsLoading(false)
    }
  }

  const filteredBlogs = blogs.filter((blog) =>
    blog.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    blog.excerpt.toLowerCase().includes(searchQuery.toLowerCase())
  )

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Blogs</h1>
        <p className="text-gray-600">Read our latest articles and insights</p>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-sm text-red-600">
          {error}
        </div>
      )}

      {/* Search */}
      <Card className="mb-6">
        <CardContent className="pt-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <Input
              placeholder="Search blogs..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10"
            />
          </div>
        </CardContent>
      </Card>

      {/* Blogs Grid */}
      {isLoading ? (
        <div className="text-center py-12 text-gray-500">Loading blogs...</div>
      ) : filteredBlogs.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 mb-4">No blogs found</p>
          {searchQuery && (
            <Button variant="outline" onClick={() => setSearchQuery('')}>
              Clear Search
            </Button>
          )}
        </div>
      ) : (
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredBlogs.map((blog) => (
            <Card
              key={blog.id}
              className="hover:shadow-lg transition-shadow cursor-pointer"
              onClick={() => navigate(`/blogs/${blog.id}`)}
            >
              <CardContent className="p-6">
                <h3 className="text-xl font-semibold text-gray-900 mb-3 line-clamp-2">
                  {blog.title}
                </h3>
                <p className="text-gray-600 mb-4 line-clamp-3">{blog.excerpt}</p>
                <div className="flex items-center justify-between text-sm text-gray-500">
                  <div className="flex items-center gap-4">
                    <div className="flex items-center gap-1">
                      <User className="w-4 h-4" />
                      <span>{blog.authorName}</span>
                    </div>
                    <div className="flex items-center gap-1">
                      <Eye className="w-4 h-4" />
                      <span>{blog.viewCount}</span>
                    </div>
                  </div>
                  <div className="flex items-center gap-1">
                    <Calendar className="w-4 h-4" />
                    <span>{new Date(blog.publishedAt).toLocaleDateString()}</span>
                  </div>
                </div>
                <Button
                  variant="ghost"
                  className="mt-4 w-full"
                  onClick={(e) => {
                    e.stopPropagation()
                    navigate(`/blogs/${blog.id}`)
                  }}
                >
                  Read More
                  <ArrowRight className="ml-2 w-4 h-4" />
                </Button>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}

export default Blogs
